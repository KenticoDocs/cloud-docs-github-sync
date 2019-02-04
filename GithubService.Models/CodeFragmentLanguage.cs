namespace GithubService.Models
{
    public enum CodeFragmentLanguage
    {
        CUrl,
        CSharp,
        JavaScript,
        TypeScript,
        Java,
        JavaRx,
        PHP,
        Swift,
        Ruby
    }

    public static class EnumExtensions
    {
        public static string GetCommentPrefix(this CodeFragmentLanguage language)
        {
            if (language == CodeFragmentLanguage.Ruby || language == CodeFragmentLanguage.CUrl)
            {
                return "#";
            }

            return "//";
        }
    }
}
