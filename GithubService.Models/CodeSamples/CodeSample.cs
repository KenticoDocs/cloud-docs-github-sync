namespace GithubService.Models.CodeSamples
{
    public class CodeSample
    {
        public string Codename { get; set; }

        public string Content { get; set; }

        public CodeLanguage Language { get; set; }
    }
}
