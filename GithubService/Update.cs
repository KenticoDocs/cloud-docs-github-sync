using GithubService.Models;
using GithubService.Models.Webhooks;
using GithubService.Repository;
using GithubService.Services.Clients;
using GithubService.Services.Converters;
using GithubService.Services.Interfaces;
using GithubService.Services.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GithubService.Services;

namespace GithubService
{
    public static class Update
    {
        [FunctionName("kcd-github-service-update")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("Update called.");

            var configuration = new Configuration.Configuration();
            var fileParser = new FileParser();

            // Get all the files from GitHub
            var githubClient = new GithubClient(
                configuration.GithubRepositoryName,
                configuration.GithubRepositoryOwner,
                configuration.GithubAccessToken);
            var githubService = new Services.GithubService(githubClient, fileParser);

            // Read Webhook message from GitHub
            WebhookMessage webhookMessage;
            using (var streamReader = new StreamReader(request.Body, Encoding.UTF8))
            {
                var requestBody = streamReader.ReadToEnd();
                webhookMessage = JsonConvert.DeserializeObject<WebhookMessage>(requestBody);
            }

            // Get paths to added/modified/deleted files
            var parser = new WebhookParser();
            var (addedFiles, modifiedFiles, removedFiles) = parser.ExtractFiles(webhookMessage);

            var connectionString = configuration.RepositoryConnectionString;
            var codeFileRepository = await CodeFileRepository.CreateInstance(connectionString);

            var addedFragmentsFromNewFiles = await ProcessAddedFiles(addedFiles, codeFileRepository, githubService);
            var (addedFragmentsFromModifiedFiles, modifiedFragments, removedFragmentsFromModifiedFiles) = 
                await ProcessModifiedFiles(modifiedFiles, codeFileRepository, githubService, logger);
            var removedFragmentsFromDeletedFiles = await ProcessRemovedFiles(removedFiles, codeFileRepository);

            var allAddedFragments = new List<CodeFragment>()
                .Concat(addedFragmentsFromNewFiles)
                .Concat(addedFragmentsFromModifiedFiles);
            var allModifiedFragments = new List<CodeFragment>()
                .Concat(modifiedFragments);
            var allRemovedFragments = new List<CodeFragment>()
                .Concat(removedFragmentsFromModifiedFiles)
                .Concat(removedFragmentsFromDeletedFiles);

            // Store code fragment event
            var eventDataRepository = await EventDataRepository.CreateInstance(connectionString);
            await new EventDataService(eventDataRepository)
                .SaveCodeFragmentEventAsync(
                    FunctionMode.Update,
                    allAddedFragments,
                    allModifiedFragments,
                    allRemovedFragments
                );

            return new OkObjectResult("Updated.");
        }

        private static async Task<IEnumerable<CodeFragment>> ProcessAddedFiles(
            ICollection<string> addedFiles,
            ICodeFileRepository codeFileRepository,
            IGithubService githubService)
        {
            if (!addedFiles.Any())
                return Enumerable.Empty<CodeFragment>();

            var codeFiles = new List<CodeFile>();

            foreach (var filePath in addedFiles)
            {
                var codeFile = await githubService.GetCodeFileAsync(filePath);

                await codeFileRepository.StoreAsync(codeFile);
                codeFiles.Add(codeFile);
            }

            return codeFiles.SelectMany(file => file.CodeFragments);
        }

        private static async Task<(IEnumerable<CodeFragment>, IEnumerable<CodeFragment>, IEnumerable<CodeFragment>)>
            ProcessModifiedFiles(
                ICollection<string> modifiedFiles,
                ICodeFileRepository codeFileRepository,
                IGithubService githubService,
                ILogger logger)
        {
            if (!modifiedFiles.Any())
                return (Enumerable.Empty<CodeFragment>(), Enumerable.Empty<CodeFragment>(), Enumerable.Empty<CodeFragment>());

            var fragmentsToAdd = new List<CodeFragment>();
            var fragmentsToModify = new List<CodeFragment>();
            var fragmentsToRemove = new List<CodeFragment>();

            var codeConverter = new CodeConverter();

            foreach (var filePath in modifiedFiles)
            {
                var oldCodeFile = await codeFileRepository.GetAsync(filePath);

                var newCodeFile = await githubService.GetCodeFileAsync(filePath);
                await codeFileRepository.StoreAsync(newCodeFile);

                if (oldCodeFile == null)
                {
                    logger.LogWarning(
                        $"Trying to modify code file {filePath} might result in inconsistent content " +
                        "in KC because there is no known previous version of the code file.");

                    fragmentsToAdd.AddRange(newCodeFile.CodeFragments);
                }
                else
                {
                    var (newFragments, modifiedFragments, removedFragments) =
                        codeConverter.CompareFragmentLists(oldCodeFile.CodeFragments, newCodeFile.CodeFragments);

                    fragmentsToAdd.AddRange(newFragments);
                    fragmentsToModify.AddRange(modifiedFragments);
                    fragmentsToRemove.AddRange(removedFragments);
                }
            }

            return (fragmentsToAdd, fragmentsToModify, fragmentsToRemove);
        }

        private static async Task<IEnumerable<CodeFragment>> ProcessRemovedFiles(
            ICollection<string> removedFiles,
            ICodeFileRepository codeFileRepository)
        {
            if (!removedFiles.Any())
                return Enumerable.Empty<CodeFragment>();

            var codeFiles = new List<CodeFile>();

            foreach (var removedFile in removedFiles)
            {
                var archivedFile = await codeFileRepository.ArchiveAsync(removedFile);

                if (archivedFile != null)
                {
                    codeFiles.Add(archivedFile);
                }
            }

            return codeFiles.SelectMany(file => file.CodeFragments);
        }
    }
}