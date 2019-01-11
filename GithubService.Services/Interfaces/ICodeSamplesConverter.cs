using System.Collections.Generic;
using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface ICodeSamplesConverter
    {
        IEnumerable<CodenameCodeSamples> ConvertToCodenameCodeSamples(IEnumerable<CodeSampleFile> codeSampleFiles);
    }
}
