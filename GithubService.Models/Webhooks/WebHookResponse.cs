using System.Collections.Generic;

namespace GithubService.Models.Webhooks
{
    public class WebhookResponse
    {
        public List<Commit> Commits { get; set; }
    }
}
