using System;
using GithubService.Services;
using GithubService.Services.Clients;
using GithubService.Services.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GithubService
{
    public static class Initialize
    {
        [FunctionName("kcd-github-service-initialize")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("Initialize called.");

            // Get all the files from GitHub - use IGithubService.GetCodeSamplesFiles
            // Persist all code sample files using ICodeSampleFileRepository

            // Convert those files using ICodeSamplesConverter.ConvertToCodenameCodeSamples
            var codeSamplesConverter = new CodeSamplesConverter();
            var samplesByCodename = codeSamplesConverter.ConvertToCodenameCodeSamples(null);

            // Create/update appropriate KC items using IKenticoCloudService
            var kenticoCloudClient = new KenticoCloudClient(
                Environment.GetEnvironmentVariable("KenticoCloud.ContentManagementApiKey"),
                Environment.GetEnvironmentVariable("KenticoCloud.ProjectId")
            );
            var kenticoInternalCloudClient = new KenticoCloudInternalClient(
                Environment.GetEnvironmentVariable("KenticoCloud.InternalApiKey"),
                Environment.GetEnvironmentVariable("KenticoCloud.ProjectId")
            );

            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, kenticoInternalCloudClient, codeSamplesConverter);
            foreach (var codeSample in samplesByCodename)
            {
                var sample = kenticoCloudService.CreateCodeSampleItemAsync(codeSample).Result;
            }

            return new OkObjectResult("Initialized.");
        }
    }
}
