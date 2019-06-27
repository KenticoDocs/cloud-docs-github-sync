using System;
using GithubService.Models;
using GithubService.Models.Webhooks;
using GithubService.Repository;
using GithubService.Services.Clients;
using GithubService.Services.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
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

            try
            {
                var configuration = new Configuration.Configuration();
                var fileParser = new FileParser();

                // Get all the files from GitHub
                var githubClient = new GithubClient(
                    new HttpClient(),
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
                var codeFileRepository = await CodeFileRepositoryProvider.CreateCodeFileRepositoryInstance(connectionString);
                var fileProcessor = new FileProcessor(githubService, codeFileRepository);

                var addedFragmentsFromNewFiles = await fileProcessor.ProcessAddedFiles(addedFiles);
                var (addedFragmentsFromModifiedFiles, modifiedFragments, removedFragmentsFromModifiedFiles) =
                    await fileProcessor.ProcessModifiedFiles(modifiedFiles, logger);
                var removedFragmentsFromDeletedFiles = await fileProcessor.ProcessRemovedFiles(removedFiles);

                var allAddedFragments = addedFragmentsFromNewFiles
                    .Concat(addedFragmentsFromModifiedFiles);
                var allRemovedFragments = removedFragmentsFromModifiedFiles
                    .Concat(removedFragmentsFromDeletedFiles);

                // Store code fragment event
                var eventDataRepository = await EventDataRepository.CreateInstance(connectionString);
                await new EventDataService(eventDataRepository)
                    .SaveCodeFragmentEventAsync(
                        FunctionMode.Update,
                        allAddedFragments,
                        modifiedFragments,
                        allRemovedFragments
                    );

                return new OkObjectResult("Updated.");
            }
            catch (Exception exception)
            {
                // This try-catch is required for correct logging of exceptions in Azure
                var message = $"Exception: {exception.Message}\nStack: {exception.StackTrace}";

                throw new GithubServiceException(message);
            }
        }
    }
}