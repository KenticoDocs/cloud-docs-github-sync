using GithubService.Models;
using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudService
    {
        bool CreateCodeSampleItem(CodenameCodeSamples sample);

        bool UpdateCodeSampleItem(CodenameCodeSamples sample);

        bool DeleteCodeSampleItem(CodenameCodeSamples sample);
    }
}
