using GithubService.Repository;
using GithubService.Services;
using GithubService.Services.Clients;
using GithubService.Services.Converters;
using GithubService.Services.Parsers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GithubService
{
    public static class Initialize
    {
        [FunctionName("kcd-github-service-initialize")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kcd-github-service-initialize/{testAttribute?}")] HttpRequest request, string testAttribute,
            ILogger logger)
        {
            logger.LogInformation("Initialize called.");

            var configuration = new Configuration.Configuration(testAttribute);
            var fileParser = new FileParser();

            // Get all the files from GitHub
            var githubClient = new GithubClient(
                configuration.GithubRepositoryName,
                configuration.GithubRepositoryOwner,
                configuration.GithubAccessToken);
            var githubService = new Services.GithubService(githubClient, fileParser);
            var codeFiles = await githubService.GetCodeFilesAsync();

            // Persist all code sample files
            var connectionString = configuration.RepositoryConnectionString;
            var codeFileRepository = await CodeFileRepository.CreateInstance(connectionString);

            foreach (var codeFile in codeFiles)
            {
                await codeFileRepository.StoreAsync(codeFile);
            }

            var codeConverter = new CodeConverter();
            var fragmentsByCodename = codeConverter.ConvertToCodenameCodeFragments(codeFiles.SelectMany(file => file.CodeFragments));

            // Create/update appropriate KC items
            var kenticoCloudClient = new KenticoCloudClient(
                configuration.KenticoCloudProjectId,
                configuration.KenticoCloudContentManagementApiKy,
                configuration.KenticoCloudInternalApiKey);

            var kenticoCloudService = new KenticoCloudService(kenticoCloudClient, codeConverter);

            foreach (var fragments in fragmentsByCodename)
            {
                await kenticoCloudService.UpsertCodeFragmentsAsync(fragments);
            }

            return new OkObjectResult("Initialized.");
        }
    }
}
