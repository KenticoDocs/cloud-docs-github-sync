using System.Collections.Generic;
using GithubService.Models.Webhooks;

namespace GithubService.Services.Interfaces
{
    public interface IWebhookParser
    {
        IEnumerable<string> ParseAddedFiles(WebhookMessage message);

        IEnumerable<string> ParseModifiedFiles(WebhookMessage message);

        IEnumerable<string> ParseDeletedFiles(WebhookMessage message);
    }
}
