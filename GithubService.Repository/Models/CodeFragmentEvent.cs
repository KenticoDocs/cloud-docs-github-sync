using System.Collections.Generic;
using Newtonsoft.Json;

namespace GithubService.Repository.Models
{
    public class CodeFragmentEvent
    {
        [JsonProperty(PropertyName = "codeFragments")]
        public IEnumerable<CodeFragment> CodeFragments { get; set; }

        [JsonProperty(PropertyName = "mode")]
        public string Mode { get; set; }
    }
}
