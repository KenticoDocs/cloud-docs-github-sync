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

            var sampleIdentifiers = ExtractSampleIdentifiers(content, language);
            if (sampleIdentifiers.Count == 0)
            {
                return codeFile;
            }

            if(sampleIdentifiers.Count != sampleIdentifiers.Distinct().ToList().Count)
            {
                throw new ArgumentException($"Duplicate code name in the file {filePath}");
            }

            ExtractCodeSamples(content, sampleIdentifiers, language, codeFile);

            if (sampleIdentifiers.Count != codeFile.CodeFragments.Count)
            {
                throw new ArgumentException($"Incorrectly marked code sample in file {filePath}");
            }

            return codeFile;
        }

        private static string GetLanguage(string filepath)
        {
            var languageIdentifier = filepath.Split('/')[0];

            switch (languageIdentifier)
            {
                case "cUrl":
                    return CodeFragmentLanguage.Curl;
                case "c#":
                    return CodeFragmentLanguage.Net;
                case "js":
                    return CodeFragmentLanguage.JavaScript;
                case "ts":
                    return CodeFragmentLanguage.TypeScript;
                case "java":
                    return CodeFragmentLanguage.Java;
                case "javarx":
                    return CodeFragmentLanguage.JavaRx;
                case "php":
                    return CodeFragmentLanguage.Php;
                case "swift":
                    return CodeFragmentLanguage.Swift;
                case "ruby":
                    return CodeFragmentLanguage.Ruby;
                case "python":
                    return CodeFragmentLanguage.Python;
                case "shell":
                    return CodeFragmentLanguage.Shell;
                default:
                    return null;
            }
        }

        private static List<string> ExtractSampleIdentifiers(string content, string language)
        {
            var sampleIdentifiersExtractor = new Regex($"{language.GetCommentPrefix()} DocSection: (.*?)\n", RegexOptions.Compiled);
            var sampleIdentifiers = new List<string>();

            var matches = sampleIdentifiersExtractor.Matches(content);

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
            CodeFile codeFile)
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
                        Identifier = sampleIdentifier,
                        Content = matchedContent,
                        Language = language,
                    }
                );
            }
        }

        private static Regex GetCodeSamplesExtractor(IEnumerable<string> sampleIdentifiers, string language)
        {
            var codeSamplesExtractor = sampleIdentifiers.Aggregate("",
                (current, sampleIdentifier) => current + $"{language.GetCommentPrefix()} DocSection: {sampleIdentifier}\\s*?\n((.|\n)*?){language.GetCommentPrefix()} EndDocSection|");

            codeSamplesExtractor = codeSamplesExtractor.Length > 0
                ? codeSamplesExtractor.Remove(codeSamplesExtractor.Length - 1)
                : codeSamplesExtractor;

            return new Regex(codeSamplesExtractor, RegexOptions.Compiled);
        }
    }
}
