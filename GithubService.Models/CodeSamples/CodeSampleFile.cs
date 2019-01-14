using System.Collections.Generic;

namespace GithubService.Models.CodeSamples
{
    public class CodeSampleFile
    {
        public CodeSampleFile()
        {
            FilePath = "";
            CodeSamples = new List<CodeSample>();
        }

        public string FilePath { get; set; }

        public List<CodeSample> CodeSamples { get; set; }

        public override string ToString() 
            => $"FilePath: {FilePath}, CodeSamples: {string.Join(", ", CodeSamples)}";
    }
}
