using System.Collections.Generic;

namespace GithubService.Models
{
    public class CodeFile
    {
        public CodeFile()
        {
            FilePath = "";
            CodeFragments = new List<CodeFragment>();
        }

        public string FilePath { get; set; }

        public List<CodeFragment> CodeFragments { get; set; }

        public override string ToString() 
            => $"FilePath: {FilePath}, CodeSamples: {string.Join(", ", CodeFragments)}";
    }
}
