using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface IGithubService
    {
        Task<IEnumerable<CodeSampleFile>> GetCodeSampleFilesAsync();

        Task<CodeSampleFile> GetCodeSampleFileAsync(string path);
    }
}
