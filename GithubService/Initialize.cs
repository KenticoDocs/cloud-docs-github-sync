using GithubService.Services;
using GithubService.Services.Clients;
using GithubService.Services.Converters;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GithubService.Repository;
using GithubService.Services.Services;

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

            var fileParser = new FileParser();

            // Get all the files from GitHub
            var githubClient = new GithubClient(
                Environment.GetEnvironmentVariable("Github.RepositoryName"),
                Environment.GetEnvironmentVariable("Github.RepositoryOwner"),
                Environment.GetEnvironmentVariable("Github.AccessToken"));
            var githubService = new Services.GithubService(githubClient, fileParser);
            var codeSampleFiles = await githubService.GetCodeSampleFilesAsync();

            // Persist all code sample files
            var connectionString = Environment.GetEnvironmentVariable("Repository.ConnectionString");
            var codeSampleFileRepository = await CodeSampleFileRepository.CreateInstance(connectionString);

            foreach (var codeSampleFile in codeSampleFiles)
            {
                await codeSampleFileRepository.StoreAsync(codeSampleFile);
            }

            var codeSamplesConverter = new CodeSamplesConverter();
            var samplesByCodename = codeSamplesConverter.ConvertToCodenameCodeSamples(codeSampleFiles);

            // Create/update appropriate KC items
            var kenticoCloudClient = new KenticoCloudClient(
                Environment.GetEnvironmentVariable("KenticoCloud.ProjectId"),
                Environment.GetEnvironmentVariable("KenticoCloud.ContentManagementApiKey"),
                Environment.GetEnvironmentVariable("KenticoCloud.InternalApiKey")
            );

            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, codeSamplesConverter);

            foreach (var codeSample in samplesByCodename)
            {
                await kenticoCloudService.UpsertCodeBlockAsync(codeSample);
            }

            return new OkObjectResult("Initialized.");
        }
    }
}
