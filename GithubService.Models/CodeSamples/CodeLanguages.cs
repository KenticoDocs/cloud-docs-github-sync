using System;

namespace GithubService.Models.CodeSamples
{
    public enum CodeLanguage
    {
        CUrl,
        CSharp,
        Javascript,
        Typescript,
        Java,
        JavaRx,
        Swift,
        Ruby,
        Python
    }

    public static class EnumExtensions
    {
        public static string GetCodename(this CodeLanguage language)
        {
            switch (language)
            {
                case CodeLanguage.CUrl:
                    return "curl";
                case CodeLanguage.CSharp:
                    return "c_";
                case CodeLanguage.Javascript:
                    return "js";
                case CodeLanguage.Typescript:
                    return "ts";
                case CodeLanguage.Java:
                    return "java";
                case CodeLanguage.JavaRx:
                    return "javarx";
                case CodeLanguage.Swift:
                    return "swift";
                case CodeLanguage.Ruby:
                    return "ruby";
                case CodeLanguage.Python:
                    return "python";
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, "Such language does not exist");
            }
        }

        public static string GetCommentPrefix(this CodeLanguage language)
        {
            if (language == CodeLanguage.Ruby || language == CodeLanguage.Python || language == CodeLanguage.CUrl)
            {
                return "#";
            }

            return "//";
        }
    }
}
