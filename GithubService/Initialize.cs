using GithubService.Services.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GithubService
{
    public static class Initialize
    {
        [FunctionName("kcd-github-service-initialize")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("Initialize called.");

            // Get all the files from GitHub - use IGithubService.GetCodeSamplesFiles
            var githubClient = new GithubClient(
                Environment.GetEnvironmentVariable("Github.RepositoryName"),
                Environment.GetEnvironmentVariable("Github.RepositoryOwner"));
            var githubService = new Services.GithubService(githubClient, null);
            var codeSampleFiles = await githubService.GetCodeSampleFilesAsync();
            // Persist all code sample files using ICodeSampleFileRepository
            // Convert those files using ICodeSamplesConverter.ConvertToCodenameCodeSamples
            // Create/update appropriate KC items using IKenticoCloudService

            return new OkObjectResult("Initialized.");
        }
    }
}
