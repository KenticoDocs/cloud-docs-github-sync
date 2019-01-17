using System.Threading.Tasks;
using GithubService.Models.KenticoCloud;
using KenticoCloud.ContentManagement.Models.Items;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudClient
    {
        Task<ContentItemModel> GetContentItemAsync(string codename);

        Task<ContentItemModel> CreateContentItemAsync(ContentItemCreateModel contentItem);

        Task<CodeBlock> UpsertContentItemVariantAsync(CodeBlock contentItemVariant, ContentItemModel contentItem);
    }
}
