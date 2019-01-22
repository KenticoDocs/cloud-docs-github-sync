using System.Collections.Generic;
using GithubService.Models;
using GithubService.Models.KenticoCloud;

namespace GithubService.Services.Interfaces
{
    public interface ICodeSamplesConverter
    {
        IEnumerable<CodenameCodeFragments> ConvertToCodenameCodeFragments(IEnumerable<CodeFile> codeFiles);

        IEnumerable<CodenameCodeSamples> ConvertToCodenameCodeSamples(CodeSampleFile codeSampleFiles);

        CodeBlock ConvertToCodeBlock(CodenameCodeSamples codenameCodeSample);
    }
}
