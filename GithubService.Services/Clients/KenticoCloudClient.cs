using System.Threading.Tasks;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement;
using KenticoCloud.ContentManagement.Models.Items;

namespace GithubService.Services.Clients
{
    public class KenticoCloudClient: IKenticoCloudClient
    {
        private readonly ContentManagementClient _contentManagementClient;

        public KenticoCloudClient(string apiKey, string projectId)
        {
            var options = new ContentManagementOptions
            {
                ApiKey = apiKey,
                ProjectId = projectId
            };
            _contentManagementClient = new ContentManagementClient(options);
        }

        public async Task<ContentItemModel> GetContentItemAsync(string codename)
            => await _contentManagementClient.GetContentItemAsync(ContentItemIdentifier.ByCodename(codename));

        public async Task<ContentItemModel> CreateContentItemAsync(ContentItemCreateModel contentItem)
            => await _contentManagementClient.CreateContentItemAsync(contentItem);

        public async Task<CodeBlock> UpsertContentItemVariantAsync(CodeBlock contentItemVariant, ContentItemModel contentItem)
        {
            var identifier = new ContentItemVariantIdentifier(ContentItemIdentifier.ById(contentItem.Id), LanguageIdentifier.DEFAULT_LANGUAGE);
            var response = await _contentManagementClient.UpsertContentItemVariantAsync(identifier, contentItemVariant);
            return response.Elements;
        }
    }
}
