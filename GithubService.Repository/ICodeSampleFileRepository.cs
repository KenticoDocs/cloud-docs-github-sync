using GithubService.Models.CodeSamples;
using System.Threading.Tasks;

namespace GithubService.Repository
{
    public interface ICodeSampleFileRepository
    {
        Task<CodeSampleFile> GetFileAsync(string filePath);

        Task<CodeSampleFile> UpdateFileAsync(CodeSampleFile updatedFile);

        Task ArchiveFileAsync(CodeSampleFile file);

        Task AddFileAsync(CodeSampleFile newFile);
    }
}
