using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using System.Collections.Generic;
using GithubService.Models;

namespace GithubService.Services.Converters
{
    public class CodeConverter : ICodeConverter
    {
        public IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(IEnumerable<CodeFile> codeFiles)
        {
            var codenameCodeFragments = new Dictionary<string, CodenameCodeFragments>();

            foreach (var codeFile in codeFiles)
            {
                foreach (var codeFragment in codeFile.CodeFragments)
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
            }

            return codenameCodeFragments.Values;
        }

        public CodeSamples ConvertToCodeSamples(CodenameCodeFragments codenameCodeFragment) => new CodeSamples
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

        private string TryGetLanguageContent(CodeFragmentLanguage language, CodenameCodeFragments codenameCodeFragment)
            => codenameCodeFragment.CodeFragments.ContainsKey(language)
                ? codenameCodeFragment.CodeFragments[language]
                : string.Empty;
    }
}
