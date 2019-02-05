using System.Threading.Tasks;
using GithubService.Models.KenticoCloud;
using GithubService.Models;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudService
    {
        Task<CodeSamples> UpsertCodeFragmentsAsync(CodenameCodeFragments fragments);

        Task<CodeSample> UpsertCodeFragmentAsync(CodeFragment fragment);

        Task<CodeSamples> RemoveCodeFragmentsAsync(CodenameCodeFragments fragments);

        Task<CodeSample> RemoveCodeFragmentAsync(CodeFragment fragment);
    }
}
