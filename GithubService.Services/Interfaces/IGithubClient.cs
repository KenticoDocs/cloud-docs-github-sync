using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Models.Github;

namespace GithubService.Services.Interfaces
{
    public interface IGithubClient
    {
        Task<IEnumerable<GithubTreeFile>> GetTreeFilesRecursively();

        Task<string> GetBlobContentAsync(string blobId);
    }
}
