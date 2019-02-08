using GithubService.Models;
using GithubService.Models.Webhooks;
using GithubService.Repository;
using GithubService.Services;
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

namespace GithubService
{
    public static class Update
    {
        [FunctionName("kcd-github-service-update")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kcd-github-service-update/{testAttribute?}")] HttpRequest request,
            string testAttribute,
            ILogger logger)
        {
            logger.LogInformation("Update called.");

            var configuration = new Configuration.Configuration(testAttribute);
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

            var codeConverter = new CodeConverter();
            var kenticoCloudClient = new KenticoCloudClient(
                configuration.KenticoCloudProjectId,
                configuration.KenticoCloudContentManagementApiKy,
                configuration.KenticoCloudInternalApiKey);

            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, codeConverter);

            await ProcessAddedFiles(addedFiles, codeFileRepository, githubService, kenticoCloudService);
            await ProcessModifiedFiles(modifiedFiles, codeFileRepository, githubService, kenticoCloudService, logger);
            await ProcessRemovedFiles(removedFiles, codeFileRepository, kenticoCloudService);

            return new OkObjectResult("Updated.");
        }

        private static async Task ProcessAddedFiles(
            ICollection<string> addedFiles,
            ICodeFileRepository codeFileRepository,
            IGithubService githubService,
            IKenticoCloudService kenticoCloudService)
        {
            if (!addedFiles.Any())
                return;

            var codeFiles = new List<CodeFile>();

            foreach (var filePath in addedFiles)
            {
                var codeFile = await githubService.GetCodeFileAsync(filePath);

                await codeFileRepository.StoreAsync(codeFile);
                codeFiles.Add(codeFile);
            }

            var codeConverter = new CodeConverter();
            var fragmentsByCodename = codeConverter.ConvertToCodenameCodeFragments(codeFiles.SelectMany(file => file.CodeFragments));

            foreach (var fragments in fragmentsByCodename)
            {
                await kenticoCloudService.UpsertCodeFragmentsAsync(fragments);
            }
        }

        private static async Task ProcessModifiedFiles(
            ICollection<string> modifiedFiles,
            ICodeFileRepository codeFileRepository,
            IGithubService githubService,
            IKenticoCloudService kenticoCloudService,
            ILogger logger)
        {
            if (!modifiedFiles.Any())
                return;

            var fragmentsToRemove = new List<CodeFragment>();
            var fragmentsToUpsert = new List<CodeFragment>();

            var codeConverter = new CodeConverter();

            foreach (var filePath in modifiedFiles)
            {
                var oldCodeFile = await codeFileRepository.GetAsync(filePath);

                var newCodeFile = await githubService.GetCodeFileAsync(filePath);
                await codeFileRepository.StoreAsync(newCodeFile);

                if (oldCodeFile == null)
                {
                    logger.LogWarning($"Trying to modify code file {filePath} might result in inconsistent content in KC because there is no known previous version of the code file.");

                    fragmentsToUpsert.AddRange(newCodeFile.CodeFragments);
                }
                else
                {
                    var (newFragments, modifiedFragments, removedFragments) = codeConverter.CompareFragmentLists(oldCodeFile.CodeFragments, newCodeFile.CodeFragments);
                    fragmentsToUpsert.AddRange(newFragments);
                    fragmentsToUpsert.AddRange(modifiedFragments);
                    fragmentsToRemove.AddRange(removedFragments);
                }
            }

            foreach (var fragments in codeConverter.ConvertToCodenameCodeFragments(fragmentsToRemove))
            {
                await kenticoCloudService.RemoveCodeFragmentsAsync(fragments);
            }

            foreach (var fragments in codeConverter.ConvertToCodenameCodeFragments(fragmentsToUpsert))
            {
                await kenticoCloudService.UpsertCodeFragmentsAsync(fragments);
            }
        }

        private static async Task ProcessRemovedFiles(
            ICollection<string> removedFiles,
            ICodeFileRepository codeFileRepository,
            IKenticoCloudService kenticoCloudService)
        {
            if (!removedFiles.Any())
                return;

            var codeFiles = new List<CodeFile>();

            foreach (var removedFile in removedFiles)
            {
                var archivedFile = await codeFileRepository.ArchiveAsync(removedFile);

                if (archivedFile != null)
                {
                    codeFiles.Add(archivedFile);
                }
            }

            var codeConverter = new CodeConverter();
            var fragmentsByCodename = codeConverter.ConvertToCodenameCodeFragments(codeFiles.SelectMany(file => file.CodeFragments));

            foreach (var fragments in fragmentsByCodename)
            {
                await kenticoCloudService.RemoveCodeFragmentsAsync(fragments);
            }
        }
    }
}
