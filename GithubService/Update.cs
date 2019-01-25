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

            var codeConverter = new CodeConverter();
            var kenticoCloudClient = new KenticoCloudClient(
                Environment.GetEnvironmentVariable("KenticoCloud.ProjectId"),
                Environment.GetEnvironmentVariable("KenticoCloud.ContentManagementApiKey"),
                Environment.GetEnvironmentVariable("KenticoCloud.InternalApiKey")
            );

            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, codeConverter);

            ProcessAddedFiles(addedFiles, codeFileRepository, githubService, kenticoCloudService, codeConverter);
            ProcessModifiedFiles(modifiedFiles, codeFileRepository, githubService, kenticoCloudService, codeConverter);
            ProcessRemovedFiles(removedFiles.ToArray(), codeFileRepository, kenticoCloudService);

            // Parse the webhook message using IWebhookParser
            // Get the affected files using IGithubService.GetCodeSamplesFile
            // Persist all code sample files using ICodeSampleFileRepository
            // Convert those files using ICodeSamplesConverter.ConvertToCodenameCodeSamples
            // Create/update appropriate KC items using IKenticoCloudService

            return new OkObjectResult("Updated.");
        }

        private static async void ProcessAddedFiles(IEnumerable<string> addedFiles, 
            ICodeFileRepository codeFileRepository, IGithubService githubService, 
            IKenticoCloudService kenticoCloudService, ICodeConverter codeConverter)
        {
            if (addedFiles.ToList().Count > 0)
            {
                foreach (var filePath in addedFiles.ToList())
                {
                    // Persist each added file
                    var codeFile = await githubService.GetCodeFileAsync(filePath);
                    await codeFileRepository.StoreAsync(codeFile);

                    // Create new KC content items
                    var codenameCodeFragments = codeConverter.ConvertToCodenameCodeFragments(codeFile);

                    foreach (var codenameCodeSample in codenameCodeSamples)
                    {
                        await kenticoCloudService.UpsertCodeBlockAsync(codenameCodeSample);
                    }
                }
            }
        }

        private static async void ProcessModifiedFiles(IEnumerable<string> modifiedFiles,
            ICodeFileRepository codeFileRepository, IGithubService githubService,
            IKenticoCloudService kenticoCloudService, ICodeConverter codeConverter)
        {
            if (modifiedFiles.ToList().Count > 0)
            {
                foreach (var filePath in modifiedFiles.ToList())
                {
                    var modifiedCodeFile = await githubService.GetCodeFileAsync(filePath);
                    var storedCodeFile = await codeFileRepository.GetAsync(filePath);

                    // In table storage replace whole file entity
                    await codeFileRepository.StoreAsync(modifiedCodeFile);

                    var modifiedCodenameCodeFragments = codeConverter.ConvertToCodenameCodeFragments(modifiedCodeFile);

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

        private static async void ProcessRemovedFiles(ICollection<string> removedFiles,
            ICodeFileRepository codeFileRepository, IKenticoCloudService kenticoCloudService)
        {
            if (!removedFiles.Any())
                return;

            foreach (var removedFile in removedFiles)
            {
                var archivedFile = await codeFileRepository.ArchiveAsync(removedFile);

                if (archivedFile == null)
                    continue;

                foreach (var codeFragment in archivedFile.CodeFragments)
                {
                    await kenticoCloudService.RemoveCodeFragmentAsync(codeFragment);
                }
            }
        }
    }
}
