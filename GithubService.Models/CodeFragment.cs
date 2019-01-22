namespace GithubService.Models
{
    public class CodeFragment
    {
        public string Codename { get; set; }

        public string Content { get; set; }

        public CodeFragmentLanguage Language { get; set; }

        public CodeFragmentType Type { get; set; }

        public override string ToString() 
            => $"Codename: {Codename}, Type: {Type}, Content: {Content}, Language: {Language}";
    }
}
