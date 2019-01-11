using GithubService.Models.CodeSamples;
using System.Threading.Tasks;

namespace GithubService.Repository
{
    public interface ICodeSampleFileRepository
    {
        Task<CodeSampleFile> GetFileAsync(string filePath);

        Task<CodeSampleFile> UpdateFileAsync(CodeSampleFile updatedFile);

        Task<CodeSampleFile> ArchiveFileAsync(CodeSampleFile file);

        Task<CodeSampleFile> AddFileAsync(CodeSampleFile newFile);
    }
}
