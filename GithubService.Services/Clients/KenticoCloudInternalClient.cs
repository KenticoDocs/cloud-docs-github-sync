using GithubService.Services.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GithubService.Services.Clients
{
    internal class KenticoCloudInternalClient : IKenticoCloudInternalClient
    {
        private readonly string _apiEndpoint;
        private readonly HttpClient _httpClient;

        public KenticoCloudInternalClient(string projectId, string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            _apiEndpoint = $"https://app.kenticocloud.com/api/project/{projectId}/item";
        }

        public async Task CreateNewVersionOfDefaultVariantAsync(Guid contentItemId)
        {
            var url = $"{_apiEndpoint}/{contentItemId}/variant/00000000-0000-0000-0000-000000000000/new-version";

            var response = await _httpClient.PutAsync(url, new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            var contentResult = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(contentResult);
        }
    }
}
