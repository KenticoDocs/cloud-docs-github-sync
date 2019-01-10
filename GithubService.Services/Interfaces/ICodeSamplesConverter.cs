using System.Collections.Generic;
using GithubService.Models;
using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface ICodeSamplesConverter
    {
        IEnumerable<CodenameCodeSamples> ConvertToCodenameCodeSampleses(IEnumerable<FileCodeSamples> samplesInFiles);
    }
}
