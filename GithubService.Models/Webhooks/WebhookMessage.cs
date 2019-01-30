using System.Collections.Generic;

namespace GithubService.Models.Webhooks
{
    public class WebhookMessage
    {
        public List<Commit> Commits { get; set; }
    }
}
