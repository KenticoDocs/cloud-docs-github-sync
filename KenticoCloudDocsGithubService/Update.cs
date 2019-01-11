using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GithubService
{
    public static class Update
    {
        [FunctionName("kcd-github-service-update")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("Update called.");

            // Parse the webhook message using IWebhookParser
            // Get the affected files using IGithubService.GetCodeSamplesFile
            // Persist all code sample files using ICodeSampleFileRepository
            // Convert those files using ICodeSamplesConverter.ConvertToCodenameCodeSamples
            // Create/update appropriate KC items using IKenticoCloudService

            return new OkObjectResult("Updated.");
        }
    }
}
