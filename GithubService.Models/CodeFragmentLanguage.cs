namespace GithubService.Models
{
    public static class CodeFragmentLanguage
    {
        public const string Curl = "curl";
        public const string Net = "_net";
        public const string JavaScript = "javascript";
        public const string TypeScript = "typescript";
        public const string Java = "java";
        public const string JavaRx = "javarx";
        public const string Php = "php";
        public const string Swift = "swift";
        public const string Ruby = "ruby";
        public const string Shell = "shell";
        public const string Python = "python";
    }

    public static class StringExtensions
    {
        public static string GetCommentPrefix(this string language)
        {
            if (language == CodeFragmentLanguage.Ruby ||
                language == CodeFragmentLanguage.Curl ||
                language == CodeFragmentLanguage.Shell ||
                language == CodeFragmentLanguage.Python)
            {
                return "#";
            }

            return "//";
        }

        public static string GetLanguageCodenameTag(this string language)
        {
            switch(language)
            {
                case CodeFragmentLanguage.JavaScript:
                    return "js";

                case CodeFragmentLanguage.TypeScript:
                    return "ts";

                case CodeFragmentLanguage.Net:
                    return "net";

                default:
                    return language;
            }
        }
    }
}