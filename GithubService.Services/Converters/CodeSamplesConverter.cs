using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using System.Collections.Generic;
using GithubService.Models;

namespace GithubService.Services.Converters
{
    public class CodeSamplesConverter : ICodeSamplesConverter
    {
        public IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(IEnumerable<CodeFile> codeFiles)
        {
            var codenameCodeFragments = new Dictionary<string, CodenameCodeFragments>();

            foreach (var codeFile in codeFiles)
            {
                var currentCodenameCodeFragments = ConvertCodeFileToCodenameCodeFragments(codeFile);

                foreach (var (codename, codenameCodeFragment) in currentCodenameCodeFragments)
                {
                    if (codenameCodeFragments.ContainsKey(codename))
                    {
                        foreach (var codeFragment in codenameCodeFragment.CodeFragments)
                        {
                            codenameCodeFragments[codename].CodeFragments.Add(codeFragment.Key, codeFragment.Value);
                        }
                    } 
                    else
                    {
                        codenameCodeFragments.Add(codename, codenameCodeFragment);
                    }

                }                
            }

            return codenameCodeFragments.Values;
        }

        public IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(CodeFile codeFiles)
            => ConvertCodeFileToCodenameCodeFragments(codeFiles).Values;
       
        private Dictionary<string, CodenameCodeFragments> ConvertCodeFileToCodenameCodeFragments(CodeFile codeFile)
        {
            var codenameCodeFragments = new Dictionary<string, CodenameCodeFragments>();

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

            return codenameCodeFragments;
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
