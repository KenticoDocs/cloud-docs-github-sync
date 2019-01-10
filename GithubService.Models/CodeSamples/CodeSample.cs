using System;

namespace GithubService.Models.CodeSamples
{
    public class CodeSample
    {
        public Guid Id { get; set; }

        public string Codename { get; set; }

        public string Content { get; set; }

        public CodeLanguage Language { get; set; }
    }
}
