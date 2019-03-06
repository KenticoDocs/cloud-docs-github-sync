using System.Threading.Tasks;
using GithubService.Models.KenticoCloud;
using GithubService.Models;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudService
    {
        Task<CodeSample> UpsertCodeFragmentAsync(CodeFragment fragment);

        Task<CodeSamples> UpsertCodenameCodeFragmentsAsync(CodenameCodeFragments fragments);

        Task<CodeSample> RemoveCodeFragmentAsync(CodeFragment fragment);
    }
}
