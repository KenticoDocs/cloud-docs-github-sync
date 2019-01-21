using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using System.Collections.Generic;

namespace GithubService.Services.Converters
{
    public class CodeSamplesConverter : ICodeSamplesConverter
    {
        public IEnumerable<CodenameCodeSamples> ConvertToCodenameCodeSamples(
            IEnumerable<CodeSampleFile> codeSampleFiles)
        {
            var codenameCodeSamples = new Dictionary<string, CodenameCodeSamples>();

            foreach (var codeSampleFile in codeSampleFiles)
            {
                foreach (var codeSample in codeSampleFile.CodeSamples)
                {
                    var codename = codeSample.Codename;
                    var language = codeSample.Language;
                    var content = codeSample.Content;

                    if (codenameCodeSamples.ContainsKey(codename))
                    {
                        codenameCodeSamples[codename].CodeSamples.Add(language, content);
                    }
                    else
                    {
                        codenameCodeSamples.Add(codename, new CodenameCodeSamples
                        {
                            Codename = codename,
                            CodeSamples = new Dictionary<CodeLanguage, string>
                            {
                                {language, content}
                            }
                        });
                    }
                }
            }

            return codenameCodeSamples.Values;
        }

        public CodeBlock ConvertToCodeBlock(CodenameCodeSamples codenameCodeSample)
        {
            return new CodeBlock
            {
                CSharp = TryGetLanguageContent(CodeLanguage.CSharp, codenameCodeSample),
                Curl = TryGetLanguageContent(CodeLanguage.CUrl, codenameCodeSample),
                Java = TryGetLanguageContent(CodeLanguage.Java, codenameCodeSample),
                Javarx = TryGetLanguageContent(CodeLanguage.JavaRx, codenameCodeSample),
                Js = TryGetLanguageContent(CodeLanguage.Javascript, codenameCodeSample),
                Swift = TryGetLanguageContent(CodeLanguage.Swift, codenameCodeSample),
                Python = TryGetLanguageContent(CodeLanguage.Python, codenameCodeSample),
                Ruby = TryGetLanguageContent(CodeLanguage.Ruby, codenameCodeSample),
                Ts = TryGetLanguageContent(CodeLanguage.Typescript, codenameCodeSample),
            };
        }

        private string TryGetLanguageContent(CodeLanguage language, CodenameCodeSamples codenameCodeSample)
            => codenameCodeSample.CodeSamples.ContainsKey(language)
                ? codenameCodeSample.CodeSamples[language]
                : string.Empty;
    }
}
