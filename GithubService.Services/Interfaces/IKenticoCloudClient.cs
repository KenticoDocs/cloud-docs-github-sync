using GithubService.Models.KenticoCloud;
using KenticoCloud.ContentManagement.Models.Items;
using System.Threading.Tasks;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudClient
    {
        Task<ContentItemModel> GetContentItemAsync(string codename);

        Task<ContentItemModel> CreateContentItemAsync(ContentItemCreateModel contentItem);

        Task<CodeSamples> GetCodeSamplesVariantAsync(ContentItemModel contentItem);

        Task<CodeSamples> UpsertCodeSamplesVariantAsync(ContentItemModel contentItem, CodeSamples codeSamples);

        Task CreateNewVersionOfDefaultVariantAsync(ContentItemModel contentItem);
    }
}
