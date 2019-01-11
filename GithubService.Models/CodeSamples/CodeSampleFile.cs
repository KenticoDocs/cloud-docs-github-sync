using System.Collections.Generic;

namespace GithubService.Models.CodeSamples
{
    public class CodeSampleFile
    {
        public string FilePath { get; set; }

        public List<CodeSample> CodeSamples { get; set; }
    }
}
