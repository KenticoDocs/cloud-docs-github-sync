using System;
using System.Collections.Generic;
using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;

namespace GithubService.Services.Converters
{
    public class CodeSamplesConverter: ICodeSamplesConverter
    {
        public IEnumerable<CodenameCodeSamples> ConvertToCodenameCodeSamples(IEnumerable<CodeSampleFile> codeSampleFiles)
        {
            throw new NotImplementedException();
        }
        public CodeBlock ConvertToCodeBlock(CodenameCodeSamples codenameCodeSample)
        {
            return new CodeBlock
            {
                Identifier = codenameCodeSample.Codename,
                C = TryGetLanguageContent(CodeLanguage.CSharp, codenameCodeSample),
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
