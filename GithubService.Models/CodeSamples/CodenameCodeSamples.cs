using System.Collections.Generic;

namespace GithubService.Models.CodeSamples
{
    public class CodenameCodeSamples
    {
        public string Codename { get; set; }

        public Dictionary<CodeLanguage, string> CodeSamples { get; set; }

        public override string ToString()
            => $"Codename: {Codename}, CodeSamples: {string.Join(", ", CodeSamples)}";
    }
}
