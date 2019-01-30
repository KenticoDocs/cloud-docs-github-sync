using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Models;

namespace GithubService.Services.Interfaces
{
    public interface IGithubService
    {
        Task<IEnumerable<CodeFile>> GetCodeFilesAsync();

        Task<CodeFile> GetCodeFileAsync(string path);
    }
}
