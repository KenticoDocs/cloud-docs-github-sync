namespace GithubService.Models
{
    public static class CodeFragmentPlatform
    {
        public const string Rest = "rest";
        public const string Net = "_net";
        public const string JavaScript = "javascript";
        public const string TypeScript = "typescript";
        public const string Java = "java";
        public const string Android = "android";
        public const string iOS = "ios";
        public const string Php = "php";
        public const string Ruby = "ruby";
        public const string React = "react";
    }

    public static class CodeFragmentPlatformExtensions
    {
        public static string GetPlatformCodenameTag(this string platform)
        {
            switch (platform)
            {
                case CodeFragmentPlatform.JavaScript:
                    return "js";

                case CodeFragmentPlatform.TypeScript:
                    return "ts";

                case CodeFragmentPlatform.Net:
                    return "net";

                default:
                    return platform;
            }
        }
    }
}