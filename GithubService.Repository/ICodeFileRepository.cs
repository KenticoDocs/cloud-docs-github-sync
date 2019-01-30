using System.Threading.Tasks;
using GithubService.Models;

namespace GithubService.Repository
{
    public interface ICodeFileRepository
    {
        Task<CodeFile> GetAsync(string filePath);

        Task<CodeFile> StoreAsync(CodeFile file);

        Task ArchiveAsync(CodeFile file);
    }
}
