using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement;
using KenticoCloud.ContentManagement.Models.Items;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GithubService.Services.Clients
{
    public class KenticoCloudClient : IKenticoCloudClient
    {
        private readonly ContentManagementClient _contentManagementClient;
        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint;

        public KenticoCloudClient(string projectId, string contentManagementApiKey)
        {
            var options = new ContentManagementOptions
            {
                ApiKey = contentManagementApiKey,
                ProjectId = projectId
            };
            _contentManagementClient = new ContentManagementClient(options);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", contentManagementApiKey);

            _apiEndpoint = $"https://manage.kenticocloud.com/v2/projects/{projectId}/items";
        }

        public async Task<ContentItemModel> GetContentItemAsync(string codename)
            => await _contentManagementClient.GetContentItemAsync(ContentItemIdentifier.ByCodename(codename));

        public async Task<ContentItemModel> CreateContentItemAsync(ContentItemCreateModel contentItem)
            => await _contentManagementClient.CreateContentItemAsync(contentItem);
        
        public async Task<T> GetVariantAsync<T>(ContentItemModel contentItem) where T : new()
        {
            var identifier = new ContentItemVariantIdentifier(ContentItemIdentifier.ById(contentItem.Id), LanguageIdentifier.DEFAULT_LANGUAGE);
            var response = await _contentManagementClient.GetContentItemVariantAsync<T>(identifier);
            return response.Elements;
        }

        public async Task<T> UpsertVariantAsync<T>(ContentItemModel contentItem, T variant) where T : new()
        {
            var identifier = new ContentItemVariantIdentifier(ContentItemIdentifier.ById(contentItem.Id), LanguageIdentifier.DEFAULT_LANGUAGE);
            var response = await _contentManagementClient.UpsertContentItemVariantAsync(identifier, variant);
            return response.Elements;
        }

        public async Task CreateNewVersionOfDefaultVariantAsync(ContentItemModel contentItem)
        {
            var url = $"{_apiEndpoint}/{contentItem.Id}/variants/00000000-0000-0000-0000-000000000000/new-version";

            var response = await _httpClient.PutAsync(url, new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            var contentResult = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.NoContent)
                throw new Exception(contentResult);
        }
    }
}
