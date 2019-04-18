using System.Collections.Generic;
using GithubService.Models.Webhooks;

namespace GithubService.Services.Interfaces
{
    public interface IWebhookParser
    {
        (ICollection<string> addedFiles, ICollection<string> modifiedFiles, ICollection<string> removedFiles) ExtractFiles(WebhookMessage message);
    }
}
