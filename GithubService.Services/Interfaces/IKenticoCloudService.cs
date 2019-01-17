using System.Threading.Tasks;
using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudService
    {
        Task<CodeBlock> CreateCodeSampleItemAsync(CodenameCodeSamples sample);

        bool UpdateCodeSampleItem(CodenameCodeSamples sample);

        bool DeleteCodeSampleItem(CodenameCodeSamples sample);
    }
}
