using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GithubService.Models;
using GithubService.Services.Interfaces;

namespace GithubService.Services.Parsers
{
    public class FileParser : IFileParser
    {
        public CodeFile ParseContent(string filePath, string content)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }
            var codeFile = new CodeFile
            {
                FilePath = filePath
            };

            if (string.IsNullOrEmpty(content))
            {
                return codeFile;
            }

            var language = GetLanguage(filePath);
            if (language == null)
            {
                return codeFile;
            }

            var platform = GetPlatform(filePath);
            if (platform == null)
            {
                return codeFile;
            }

            var sampleIdentifiers = ExtractSampleIdentifiers(content, language);
            if (sampleIdentifiers.Count == 0)
            {
                return codeFile;
            }

            if (sampleIdentifiers.Count != sampleIdentifiers.Distinct().ToList().Count)
            {
                throw new ArgumentException($"Duplicate codename in the file {filePath}");
            }

            ExtractCodeSamples(content, sampleIdentifiers, language, codeFile, platform);

            if (sampleIdentifiers.Count != codeFile.CodeFragments.Count)
            {
                throw new ArgumentException($"Incorrectly marked code sample in file {filePath}");
            }

            return codeFile;
        }

        private static string GetPlatform(string filepath)
        {
            var platformIdentifier = filepath.Split('/')[0];

            switch (platformIdentifier)
            {
                case "rest":
                    return CodeFragmentPlatform.Rest;
                case "net":
                    return CodeFragmentPlatform.Net;
                case "js":
                    return CodeFragmentPlatform.JavaScript;
                case "ts":
                    return CodeFragmentPlatform.TypeScript;
                case "java":
                    return CodeFragmentPlatform.Java;
                case "android":
                    return CodeFragmentPlatform.Android;
                case "ios":
                    return CodeFragmentPlatform.iOS;
                case "php":
                    return CodeFragmentPlatform.Php;
                case "ruby":
                    return CodeFragmentPlatform.Ruby;
                case "react":
                    return CodeFragmentPlatform.React;
                default:
                    return null;
            }
        }

        private static string GetLanguage(string filepath)
        {
            var splittedFilePath = filepath.Split('.');
            var languageIdentifier = splittedFilePath[splittedFilePath.Length - 1];

            switch (languageIdentifier)
            {
                case "cs":
                    return CodeFragmentLanguage.CSharp;
                case "css":
                    return CodeFragmentLanguage.Css;
                case "html":
                case "cshtml":
                    return CodeFragmentLanguage.HTML;
                case "java":
                    return CodeFragmentLanguage.Java;
                case "js":
                case "jsx":
                    return CodeFragmentLanguage.JavaScript;
                case "php":
                    return CodeFragmentLanguage.Php;
                case "py":
                    return CodeFragmentLanguage.Python;
                case "rb":
                    return CodeFragmentLanguage.Ruby;
                case "swift":
                    return CodeFragmentLanguage.Swift;
                case "ts":
                case "tsx":
                    return CodeFragmentLanguage.TypeScript;
                case "sh":
                    return CodeFragmentLanguage.Shell;
                case "curl":
                    return CodeFragmentLanguage.Curl;
                default:
                    return null;
            }
        }

        private static List<string> ExtractSampleIdentifiers(string content, string language)
        {
            var sampleIdentifiersExtractor = new Regex($"{language.GetCommentPrefix()} DocSection: ([\\w_]*)", RegexOptions.Compiled);
            var sampleIdentifiers = new List<string>();

            var matches = sampleIdentifiersExtractor.Matches(content.Trim());

            foreach (Match match in matches)
            {
                sampleIdentifiers.Add(match.Groups[1].Value.Trim());
            }

            return sampleIdentifiers;
        }

        private static void ExtractCodeSamples(
            string content,
            IReadOnlyList<string> sampleIdentifiers,
            string language,
            CodeFile codeFile,
            string platform)
        {
            var codeSamplesExtractor = GetCodeSamplesExtractor(sampleIdentifiers, language);
            var codeSamplesFileMatches = codeSamplesExtractor.Matches(content);

            var matchedGroupIndex = 1;
            for (var index = 0; index < codeSamplesFileMatches.Count; index++)
            {
                var sampleIdentifier = sampleIdentifiers[index];
                var matchedContent = codeSamplesFileMatches[index]
                    .Groups[matchedGroupIndex]
                    .Value
                    .Trim();

                if (matchedContent.Contains($"{language.GetCommentPrefix()} DocSection: "))
                {
                    throw new ArgumentException("Nested or intersected marking of code samples found");
                }

                matchedGroupIndex += 2;

                codeFile.CodeFragments.Add(new CodeFragment
                {
                    Identifier = sampleIdentifier.ToLower(),
                    Content = matchedContent,
                    Language = language,
                    Platform = platform
                }
                );
            }
        }

        private static Regex GetCodeSamplesExtractor(IEnumerable<string> sampleIdentifiers, string language)
        {
            var codeSamplesExtractor = sampleIdentifiers.Aggregate("",
                (current, sampleIdentifier) => current + $"{language.GetCommentPrefix()} DocSection: {sampleIdentifier}{language.GetCommentSuffix()}\\s*?\n((.|\n)*?){language.GetCommentPrefix()} EndDocSection{language.GetCommentSuffix()}|");

            codeSamplesExtractor = codeSamplesExtractor.Length > 0
                ? codeSamplesExtractor.Remove(codeSamplesExtractor.Length - 1)
                : codeSamplesExtractor;

            return new Regex(codeSamplesExtractor, RegexOptions.Compiled);
        }
    }
}
