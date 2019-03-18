namespace GithubService.Models
{
    public static class CodeFragmentLanguage
    {
        public const string CSharp = "c_";
        public const string Css = "css";
        public const string Curl = "curl";
        public const string HTML = "html";
        public const string Java = "java";
        public const string JavaRx = "javarx";
        public const string JavaScript = "javascript";
        public const string Php = "php";
        public const string Python = "python";
        public const string Ruby = "ruby";
        public const string Shell = "shell";
        public const string Swift = "swift";
        public const string TypeScript = "typescript";
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

                case CodeFragmentLanguage.CSharp:
                    return "CSharp";

                default:
                    return language;
            }
        }
    }
}
