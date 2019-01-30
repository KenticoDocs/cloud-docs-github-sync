using GithubService.Repository;
using GithubService.Services;
using GithubService.Services.Clients;
using GithubService.Services.Converters;
using GithubService.Services.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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

            var fileParser = new FileParser();

            // Get all the files from GitHub
            var githubClient = new GithubClient(
                Environment.GetEnvironmentVariable("Github.RepositoryName"),
                Environment.GetEnvironmentVariable("Github.RepositoryOwner"),
                Environment.GetEnvironmentVariable("Github.AccessToken"));
            var githubService = new Services.GithubService(githubClient, fileParser);
            var codeFiles = await githubService.GetCodeFilesAsync();

            // Persist all code sample files
            var connectionString = Environment.GetEnvironmentVariable("Repository.ConnectionString");
            var codeFileRepository = await CodeFileRepository.CreateInstance(connectionString);

            foreach (var codeFile in codeFiles)
            {
                await codeFileRepository.StoreAsync(codeFile);
            }

            var codeConverter = new CodeConverter();
            var fragmentsByCodename = codeConverter.ConvertToCodenameCodeFragments(codeFiles.SelectMany(file => file.CodeFragments));

            // Create/update appropriate KC items
            var kenticoCloudClient = new KenticoCloudClient(
                Environment.GetEnvironmentVariable("KenticoCloud.ProjectId"),
                Environment.GetEnvironmentVariable("KenticoCloud.ContentManagementApiKey"),
                Environment.GetEnvironmentVariable("KenticoCloud.InternalApiKey")
            );

            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, codeConverter);

            foreach (var fragments in fragmentsByCodename)
            {
                await kenticoCloudService.UpsertCodeFragmentsAsync(fragments);
            }

            return new OkObjectResult("Initialized.");
        }
    }
}
