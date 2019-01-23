using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GithubService.Models.Webhooks;
using GithubService.Repository;
using GithubService.Services;
using GithubService.Services.Clients;
using GithubService.Services.Converters;
using GithubService.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using GithubService.Services.Interfaces;

namespace GithubService
{
    public static class Update
    {
        [FunctionName("kcd-github-service-update")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("Update called.");

            var fileParser = new FileParser();

            // Get all the files from GitHub
            var githubClient = new GithubClient(
                Environment.GetEnvironmentVariable("Github.RepositoryName"),
                Environment.GetEnvironmentVariable("Github.RepositoryOwner"),
                Environment.GetEnvironmentVariable("Github.AccessToken"));
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

            var connectionString = Environment.GetEnvironmentVariable("Repository.ConnectionString");
            var codeSampleFileRepository = await CodeSampleFileRepository.CreateInstance(connectionString);

            var codeSamplesConverter = new CodeSamplesConverter();
            var kenticoCloudClient = new KenticoCloudClient(
                Environment.GetEnvironmentVariable("KenticoCloud.ProjectId"),
                Environment.GetEnvironmentVariable("KenticoCloud.ContentManagementApiKey"),
                Environment.GetEnvironmentVariable("KenticoCloud.InternalApiKey")
            );

            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, codeSamplesConverter);

            ProcessAddedFiles(addedFiles, codeSampleFileRepository, githubService, kenticoCloudService, codeSamplesConverter);
            ProcessModifiedFiles(modifiedFiles, codeSampleFileRepository, githubService, kenticoCloudService, codeSamplesConverter);


            // Parse the webhook message using IWebhookParser
            // Get the affected files using IGithubService.GetCodeSamplesFile
            // Persist all code sample files using ICodeSampleFileRepository
            // Convert those files using ICodeSamplesConverter.ConvertToCodenameCodeSamples
            // Create/update appropriate KC items using IKenticoCloudService

            return new OkObjectResult("Updated.");
        }

        private static async void ProcessAddedFiles(IEnumerable<string> addedFiles, 
            ICodeSampleFileRepository codeSampleFileRepository, IGithubService githubService, 
            IKenticoCloudService kenticoCloudService, ICodeSamplesConverter codeSamplesConverter)
        {
            if (addedFiles.ToList().Count > 0)
            {
                foreach (var filePath in addedFiles.ToList())
                {
                    // Persist each added file
                    var codeSampleFile = await githubService.GetCodeSampleFileAsync(filePath);
                    await codeSampleFileRepository.StoreAsync(codeSampleFile);

                    // Create new KC content items
                    var codenameCodeSamples = codeSamplesConverter.ConvertToCodenameCodeSamples(codeSampleFile);

                    foreach (var codenameCodeSample in codenameCodeSamples)
                    {
                        await kenticoCloudService.UpsertCodeBlockAsync(codenameCodeSample);
                    }
                }
            }
        }

        private static async void ProcessModifiedFiles(IEnumerable<string> modifiedFiles,
            ICodeSampleFileRepository codeSampleFileRepository, IGithubService githubService,
            IKenticoCloudService kenticoCloudService, ICodeSamplesConverter codeSamplesConverter)
        {
            if (modifiedFiles.ToList().Count > 0)
            {
                foreach (var filePath in modifiedFiles.ToList())
                {
                    var modifiedCodeSampleFile = await githubService.GetCodeSampleFileAsync(filePath);
                    var storedCodeSampleFile = await codeSampleFileRepository.GetAsync(filePath);

                    // In table storage replace whole file entity
                    await codeSampleFileRepository.StoreAsync(modifiedCodeSampleFile);

                    var modifiedCodenameCodeSamples = codeSamplesConverter.ConvertToCodenameCodeSamples(modifiedCodeSampleFile);

                    foreach (var storedCodeSample in storedCodeSampleFile.CodeSamples)
                    {
                        var removedCodeSample = modifiedCodenameCodeSamples
                            .Where(codeSample => codeSample.Codename == storedCodeSample.Codename)
                            .First();

                        if (!modifiedCodeSampleFile.CodeSamples.Contains(storedCodeSample))
                        {
                            kenticoCloudService.DeleteCodeSampleItem(removedCodeSample);
                        }
                    }

                    foreach (var modifiedCodeSample in modifiedCodenameCodeSamples)
                    {
                        await kenticoCloudService.UpsertCodeBlockAsync(modifiedCodeSample);
                    }
                }
            }
        }
    }
}
