using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GithubService
{
    public static class GithubFunction
    {
        [FunctionName("kcd-github-service")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string mode = request.Query["mode"];
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();

            if (mode == "initialize")
            {
                InitializeCodeSamples();
            } else if (mode == "webhook")
            {
                ProccessWebhookResponse();
            }
            else
            {
                return new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }

            return new OkObjectResult("Webhook done");
        }

        private static void InitializeCodeSamples()
        {

        }

        private static void ProccessWebhookResponse()
        {

        }
    }
}
