using System.Collections.Generic;

namespace GithubService.Models
{
    public class CodenameCodeFragments
    {
        public string Codename { get; set; }

        public Dictionary<CodeFragmentLanguage, string> CodeFragments { get; set; }
    }
}
