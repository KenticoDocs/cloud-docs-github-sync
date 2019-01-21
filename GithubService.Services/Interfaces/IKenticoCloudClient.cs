using GithubService.Models.KenticoCloud;
using KenticoCloud.ContentManagement.Models.Items;
using System.Threading.Tasks;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudClient
    {
        Task<ContentItemModel> GetContentItemAsync(string codename);

        Task<ContentItemModel> CreateContentItemAsync(ContentItemCreateModel contentItem);

        Task<CodeBlock> UpsertCodeBlockVariantAsync(ContentItemModel contentItem, CodeBlock codeBlock);

        Task CreateNewVersionOfDefaultVariantAsync(ContentItemModel contentItem);
    }
}
