using System.Collections.Generic;
using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface IGithubService
    {
        IEnumerable<CodeSampleFile> GetCodeSamplesFiles();

        CodeSampleFile GetCodeSamplesFile(string path);
    }
}
