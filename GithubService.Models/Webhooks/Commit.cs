using System.Collections.Generic;

namespace GithubService.Models.Webhooks
{
    public class Commit
    {
        public List<string> Added { get; set; }

        public List<string> Removed { get; set; }

        public List<string> Modified { get; set; }
    }
}
