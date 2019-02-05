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

                if (codenameCodeFragments.ContainsKey(codename))
                {
                    codenameCodeFragments[codename].CodeFragments.Add(codeFragment);
                }
                else
                {
                    codenameCodeFragments.Add(codename, new CodenameCodeFragments
                    {
                        Codename = codename,
                        CodeFragments = new List<CodeFragment> {codeFragment}
                    });
                }
            }

            return codenameCodeFragments.Values;
        }

        public CodeSamples ConvertToCodeSamples(CodenameCodeFragments codenameCodeFragments) => new CodeSamples
        {
            Curl = GetLanguageContent(CodeFragmentLanguage.Curl, codenameCodeFragments),
            CSharp = GetLanguageContent(CodeFragmentLanguage.Net, codenameCodeFragments),
            JavaScript = GetLanguageContent(CodeFragmentLanguage.JavaScript, codenameCodeFragments),
            TypeScript = GetLanguageContent(CodeFragmentLanguage.TypeScript, codenameCodeFragments),
            Java = GetLanguageContent(CodeFragmentLanguage.Java, codenameCodeFragments),
            JavaRx = GetLanguageContent(CodeFragmentLanguage.JavaRx, codenameCodeFragments),
            PHP = GetLanguageContent(CodeFragmentLanguage.Php, codenameCodeFragments),
            Swift = GetLanguageContent(CodeFragmentLanguage.Swift, codenameCodeFragments),
            Ruby = GetLanguageContent(CodeFragmentLanguage.Ruby, codenameCodeFragments)
        };

        public (List<CodeFragment> newFragments, List<CodeFragment> modifiedFragments, List<CodeFragment>
            removedFragments) CompareFragmentLists(List<CodeFragment> oldFragmentList,
                List<CodeFragment> newFragmentList)
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

        private static string GetLanguageContent(string language, CodenameCodeFragments codenameCodeFragments)
            => codenameCodeFragments.CodeFragments.FirstOrDefault(codeFragment => codeFragment.Language == language)?.Content ?? string.Empty;

        private static bool CompareCodeFragments(CodeFragment first, CodeFragment second)
            => first.Codename == second.Codename &&
               first.Language == second.Language &&
               first.Type == second.Type;
    }
}