using System;

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
        public static string GetCodename(this CodeFragmentLanguage language)
        {
            switch (language)
            {
                case CodeFragmentLanguage.CUrl:
                    return "curl";
                case CodeFragmentLanguage.CSharp:
                    return "c_";
                case CodeFragmentLanguage.JavaScript:
                    return "js";
                case CodeFragmentLanguage.TypeScript:
                    return "ts";
                case CodeFragmentLanguage.Java:
                    return "java";
                case CodeFragmentLanguage.JavaRx:
                    return "javarx";
                case CodeFragmentLanguage.PHP:
                    return "php";
                case CodeFragmentLanguage.Swift:
                    return "swift";
                case CodeFragmentLanguage.Ruby:
                    return "ruby";
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, "Such language does not exist");
            }
        }

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
