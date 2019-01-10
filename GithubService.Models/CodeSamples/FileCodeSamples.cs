using System;
using System.Collections.Generic;

namespace GithubService.Models.CodeSamples
{
    public class FileCodeSamples
    {
        public Guid Id { get; set; }

        public string FilePath { get; set; }

        public List<CodeSample> CodeSamples { get; set; }
    }
}
