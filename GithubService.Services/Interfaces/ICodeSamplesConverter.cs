using System.Collections.Generic;
using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;

namespace GithubService.Services.Interfaces
{
    public interface ICodeSamplesConverter
    {
        IEnumerable<CodenameCodeSamples> ConvertToCodenameCodeSamples(IEnumerable<CodeSampleFile> codeSampleFiles);

        CodeBlock ConvertToCodeBlock(CodenameCodeSamples codenameCodeSample);
    }
}
