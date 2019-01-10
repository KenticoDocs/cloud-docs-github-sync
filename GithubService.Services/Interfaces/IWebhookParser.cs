using System.Collections.Generic;
using GithubService.Models.Webhooks;

namespace GithubService.Services.Interfaces
{
    public interface IWebhookParser
    {
        IEnumerable<string> ParseAddedFiles(WebhookResponse webhook);

        IEnumerable<string> ParseModifiedFiles(WebhookResponse webhook);

        IEnumerable<string> ParseDeletedFiles(WebhookResponse webhook);
    }
}
