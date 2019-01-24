using GithubService.Models;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using System.Collections.Generic;

namespace GithubService.Services.Converters
{
    public class CodeSamplesConverter : ICodeSamplesConverter
    {
        public IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(
            IEnumerable<CodeFile> codeFiles)
        {
            var codenameCodeSamples = new Dictionary<string, CodenameCodeFragments>();

            foreach (var codeFile in codeFiles)
            {
                foreach (var codeFragment in codeFile.CodeFragments)
                {
                    var codename = codeFragment.Codename;
                    var language = codeFragment.Language;
                    var content = codeFragment.Content;

                    if (codenameCodeSamples.ContainsKey(codename))
                    {
                        codenameCodeSamples[codename].CodeFragments.Add(language, content);
                    }
                    else
                    {
                        codenameCodeSamples.Add(codename, new CodenameCodeFragments
                        {
                            Codename = codename,
                            CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                            {
                                {language, content}
                            }
                        });
                    }
                }
            }

            return codenameCodeSamples.Values;
        }

        public CodeSamples ConvertToCodeSamples(CodenameCodeFragments codenameCodeFragment)
        {
            return new CodeSamples
            {
                Curl = TryGetLanguageContent(CodeFragmentLanguage.CUrl, codenameCodeFragment),
                CSharp = TryGetLanguageContent(CodeFragmentLanguage.CSharp, codenameCodeFragment),
                JavaScript = TryGetLanguageContent(CodeFragmentLanguage.JavaScript, codenameCodeFragment),
                TypeScript = TryGetLanguageContent(CodeFragmentLanguage.TypeScript, codenameCodeFragment),
                Java = TryGetLanguageContent(CodeFragmentLanguage.Java, codenameCodeFragment),
                JavaRx = TryGetLanguageContent(CodeFragmentLanguage.JavaRx, codenameCodeFragment),
                PHP = TryGetLanguageContent(CodeFragmentLanguage.PHP, codenameCodeFragment),
                Swift = TryGetLanguageContent(CodeFragmentLanguage.Swift, codenameCodeFragment),
                Ruby = TryGetLanguageContent(CodeFragmentLanguage.Ruby, codenameCodeFragment)
            };
        }

        private string TryGetLanguageContent(CodeFragmentLanguage language, CodenameCodeFragments codenameCodeFragment)
            => codenameCodeFragment.CodeFragments.ContainsKey(language)
                ? codenameCodeFragment.CodeFragments[language]
                : string.Empty;
    }
}
