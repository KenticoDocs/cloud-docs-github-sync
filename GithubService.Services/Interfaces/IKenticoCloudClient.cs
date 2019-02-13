using KenticoCloud.ContentManagement.Models.Items;
using System.Threading.Tasks;

namespace GithubService.Services.Interfaces
{
    public interface IKenticoCloudClient
    {
        Task<ContentItemModel> GetContentItemAsync(string codename);

        Task<ContentItemModel> CreateContentItemAsync(ContentItemCreateModel contentItem);

        Task<T> GetVariantAsync<T>(ContentItemModel contentItem) where T : new();

        Task<T> UpsertVariantAsync<T>(ContentItemModel contentItem, T variant) where T : new();

        Task CreateNewVersionOfDefaultVariantAsync(ContentItemModel contentItem);
    }
}
