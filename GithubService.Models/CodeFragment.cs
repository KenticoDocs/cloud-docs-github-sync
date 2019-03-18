namespace GithubService.Models
{
    public class CodeFragment
    {
        public string Identifier { get; set; }

        public string Content { get; set; }

        public string Language { get; set; }

        public string Platform { get; set; }

        public string Codename
            => $"{Identifier}_{Platform.GetPlatformCodenameTag()}";

        public override string ToString() 
            => $"Identifier: {Identifier}, Content: {Content}, Language: {Language}";
    }
}
