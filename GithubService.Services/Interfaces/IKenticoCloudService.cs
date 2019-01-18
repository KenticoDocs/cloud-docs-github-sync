using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;
using System.Threading.Tasks;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudService
    {
        Task<CodeBlock> UpsertCodeBlockAsync(CodenameCodeSamples sample);

        bool DeleteCodeSampleItem(CodenameCodeSamples sample);
    }
}
