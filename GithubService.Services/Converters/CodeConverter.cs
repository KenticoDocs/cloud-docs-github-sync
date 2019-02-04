using GithubService.Models;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GithubService.Services.Converters
{
    public class CodeConverter : ICodeConverter
    {
        public IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(IEnumerable<CodeFragment> fragments)
        {
            var codenameCodeFragments = new Dictionary<string, CodenameCodeFragments>();

            foreach (var codeFragment in fragments)
            {
                var codename = codeFragment.Codename;
                var language = codeFragment.Language;
                var content = codeFragment.Content;

                if (codenameCodeFragments.ContainsKey(codename))
                {
                    codenameCodeFragments[codename].CodeFragments.Add(language, content);
                }
                else
                {
                    codenameCodeFragments.Add(codename, new CodenameCodeFragments
                    {
                        Codename = codename,
                        CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                            {
                                {language, content}
                            }
                    });
                }
            }

            return codenameCodeFragments.Values;
        }

        public CodeSamples ConvertToCodeSamples(CodenameCodeFragments codenameCodeFragments) => new CodeSamples
        {
            Curl = GetLanguageContent(CodeFragmentLanguage.CUrl, codenameCodeFragments),
            CSharp = GetLanguageContent(CodeFragmentLanguage.CSharp, codenameCodeFragments),
            JavaScript = GetLanguageContent(CodeFragmentLanguage.JavaScript, codenameCodeFragments),
            TypeScript = GetLanguageContent(CodeFragmentLanguage.TypeScript, codenameCodeFragments),
            Java = GetLanguageContent(CodeFragmentLanguage.Java, codenameCodeFragments),
            JavaRx = GetLanguageContent(CodeFragmentLanguage.JavaRx, codenameCodeFragments),
            PHP = GetLanguageContent(CodeFragmentLanguage.PHP, codenameCodeFragments),
            Swift = GetLanguageContent(CodeFragmentLanguage.Swift, codenameCodeFragments),
            Ruby = GetLanguageContent(CodeFragmentLanguage.Ruby, codenameCodeFragments)
        };

        public (List<CodeFragment> newFragments, List<CodeFragment> modifiedFragments, List<CodeFragment> removedFragments) CompareFragmentLists(List<CodeFragment> oldFragmentList, List<CodeFragment> newFragmentList)
        {
            var newFragments = new List<CodeFragment>();
            var modifiedFragments = new List<CodeFragment>();
            var removedFragments = new List<CodeFragment>();

            foreach (var codeFragment in oldFragmentList)
            {
                var matchingCodeFragment = newFragmentList.FirstOrDefault(cf => CompareCodeFragments(cf, codeFragment));

                if (matchingCodeFragment == null)
                {
                    // Fragment was present in the old file but it's not in the new one
                    removedFragments.Add(codeFragment);
                }
                else if (matchingCodeFragment.Content != codeFragment.Content)
                {
                    // The content of the fragment changed
                    modifiedFragments.Add(matchingCodeFragment);
                }
            }

            foreach (var codeFragment in newFragmentList)
            {
                var matchingCodeFragment = oldFragmentList.FirstOrDefault(cf => CompareCodeFragments(cf, codeFragment));

                if (matchingCodeFragment == null)
                {
                    // Fragment was added
                    newFragments.Add(codeFragment);
                }
            }

            return (newFragments, modifiedFragments, removedFragments);
        }

        public string ConvertCodenameToItemName(string codename)
        {
            if (string.IsNullOrEmpty(codename))
                return codename;

            var textInfo = new CultureInfo("en-US", false).TextInfo;

            return textInfo.ToTitleCase(codename.Replace('_', ' '));
        }

        private string GetLanguageContent(CodeFragmentLanguage language, CodenameCodeFragments codenameCodeFragments)
            => codenameCodeFragments.CodeFragments.ContainsKey(language)
                ? codenameCodeFragments.CodeFragments[language]
                : string.Empty;

        private bool CompareCodeFragments(CodeFragment first, CodeFragment second)
            => first.Codename == second.Codename &&
               first.Language == second.Language &&
               first.Type == second.Type;
    }
}
