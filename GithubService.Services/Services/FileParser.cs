using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GithubService.Models.CodeSamples;
using GithubService.Services.Interfaces;

namespace GithubService.Services.Services
{
    public class FileParser : IFileParser
    {
        public CodeSampleFile ParseContent(string filePath, string content)
        {
            CheckNullOrEmptyArguments(filePath, content);

            var codeSampleFile = new CodeSampleFile
            {
                FilePath = filePath
            };

            var extractedLanguage = GetLanguage(filePath);
            if (extractedLanguage == null)
            {
                return codeSampleFile;
            }
            var language = (CodeLanguage) extractedLanguage;

            var codenames = ExtractCodenames(content, language);
            if (codenames.Count == 0)
            {
                return codeSampleFile;
            }

            ExtractCodeSamples(content, codenames, language, codeSampleFile);

            if (codenames.Count != codeSampleFile.CodeSamples.Count)
            {
                throw new ArgumentException($"Incorrectly marked code sample in file {filePath}");
            }

            return codeSampleFile;
        }

        private static void CheckNullOrEmptyArguments(string filePath, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException($"Content of file {filePath} is either empty or null");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }
        }

        private static CodeLanguage? GetLanguage(string filepath)
        {
            var languageIdentifier = filepath.Split('/')[0];

            switch (languageIdentifier)
            {
                case "cUrl":
                    return CodeLanguage.CUrl;
                case "c#":
                    return CodeLanguage.CSharp;
                case "js":
                    return CodeLanguage.Javascript;
                case "ts":
                    return CodeLanguage.Typescript;
                case "java":
                    return CodeLanguage.Java;
                case "javarx":
                    return CodeLanguage.JavaRx;
                case "swift":
                    return CodeLanguage.Swift;
                case "ruby":
                    return CodeLanguage.Ruby;
                case "python":
                    return CodeLanguage.Python;
                default:
                    return null;
            }
        }

        private static List<string> ExtractCodenames(string content, CodeLanguage language)
        {
            var codenamesExtractor = new Regex($"{language.GetCommentPrefix()} start::(.*?)\n", RegexOptions.Compiled);
            var codenames = new List<string>();

            var matches = codenamesExtractor.Matches(content);

            foreach (Match match in matches)
            {
                codenames.Add(match.Groups[1].Value.Trim());
            }

            return codenames;
        }

        private static void ExtractCodeSamples(
            string content,
            IReadOnlyList<string> codenames,
            CodeLanguage language,
            CodeSampleFile codeSampleFile)
        {
            var codeSamplesExtractor = GetCodeSamplesExtractor(codenames, language);
            var codeSamplesFileMatches = codeSamplesExtractor.Matches(content);

            var matchedGroupIndex = 1;
            for (var index = 0; index < codeSamplesFileMatches.Count; index++)
            {
                var codename = codenames[index];
                var matchedContent = codeSamplesFileMatches[index]
                    .Groups[matchedGroupIndex]
                    .Value
                    .Trim();

                if (matchedContent.Contains($"{language.GetCommentPrefix()} start::") ||
                    matchedContent.Contains($"{language.GetCommentPrefix()} end::"))
                {
                    throw new ArgumentException("Nested or intersected marking of code samples found");
                }

                matchedGroupIndex += 2;

                codeSampleFile.CodeSamples.Add(new CodeSample
                    {
                        Codename = codename,
                        Content = matchedContent,
                        Language = language
                    }
                );
            }
        }

        private static Regex GetCodeSamplesExtractor(IEnumerable<string> codenames, CodeLanguage language)
        {
            var codeSamplesExtractor = codenames.Aggregate("",
                (current, codename) => current + $"{language.GetCommentPrefix()} start::{codename}\n*?((.|\n)*?){language.GetCommentPrefix()} end::{codename}|");

            codeSamplesExtractor = codeSamplesExtractor.Length > 0
                ? codeSamplesExtractor.Remove(codeSamplesExtractor.Length - 1)
                : codeSamplesExtractor;

            return new Regex(codeSamplesExtractor, RegexOptions.Compiled);
        }
    }
}
