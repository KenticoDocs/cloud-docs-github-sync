using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GithubService.Models.Webhooks;
using GithubService.Repository;
using GithubService.Services;
using GithubService.Services.Clients;
using GithubService.Services.Converters;
using GithubService.Services.Parsers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            // Parse the webhook message using IWebhookParser
            WebhookMessage webhookMessage;
            using (var streamReader = new StreamReader(request.Body, Encoding.UTF8))
            {
                var requestBody = streamReader.ReadToEnd();
                webhookMessage = JsonConvert.DeserializeObject<WebhookMessage>(requestBody);
            }
            var parser = new WebhookParser();
            var (addedFiles, modifiedFiles, removedFiles) = parser.ExtractFiles(webhookMessage);

            // Get the affected files using IGithubService.GetCodeSamplesFile
            // Persist all code sample files using ICodeSampleFileRepository
            // Convert those files using ICodeSamplesConverter.ConvertToCodenameCodeSamples
            // Create/update appropriate KC items using IKenticoCloudService
            var connectionString = Environment.GetEnvironmentVariable("Repository.ConnectionString");
            var codeSampleFileRepository = await CodeFileRepository.CreateInstance(connectionString);

            var kenticoCloudClient = new KenticoCloudClient(
                Environment.GetEnvironmentVariable("KenticoCloud.ProjectId"),
                Environment.GetEnvironmentVariable("KenticoCloud.ContentManagementApiKey"),
                Environment.GetEnvironmentVariable("KenticoCloud.InternalApiKey")
            );
            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, new CodeSamplesConverter());

            foreach (var removedFile in removedFiles)
            {
                var archivedFile = await codeSampleFileRepository.ArchiveAsync(removedFile);

                if (archivedFile == null)
                    continue;

                foreach (var codeFragment in archivedFile.CodeFragments)
                {
                    await kenticoCloudService.RemoveCodeFragmentAsync(codeFragment);
                }
            }

            return new OkObjectResult("Updated.");
        }
    }
}
