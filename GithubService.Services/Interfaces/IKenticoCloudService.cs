using System.Threading.Tasks;
using GithubService.Models.KenticoCloud;
using GithubService.Models;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudService
    {
        Task<CodeSamples> UpsertCodeFragmentsAsync(CodenameCodeFragments fragment);

        Task RemoveCodeFragmentAsync(CodeFragment codeFragment);
    }
}
