using System.Collections.Generic;
using GithubService.Models.Webhooks;

namespace GithubService.Services.Interfaces
{
    public interface IWebhookParser
    {
        (IEnumerable<string> addedFiles, IEnumerable<string> modifiedFiles, IEnumerable<string> removedFiles) ExtractFiles(WebhookMessage message);
    }
}
