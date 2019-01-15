using System.Collections.Generic;
using GithubService.Models.Webhooks;

namespace GithubService.Services.Interfaces
{
    public interface IWebhookParser
    {
        (IEnumerable<string>, IEnumerable<string>, IEnumerable<string>) ParseFiles(WebhookMessage message);
    }
}
