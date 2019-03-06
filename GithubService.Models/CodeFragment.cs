namespace GithubService.Models
{
    public class CodeFragment
    {
        public string Codename { get; set; }

        public string Content { get; set; }

        public string Language { get; set; }

        public override string ToString() 
            => $"Codename: {Codename}, Content: {Content}, Language: {Language}";
    }
}
