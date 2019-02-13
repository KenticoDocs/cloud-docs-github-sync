﻿using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement;
using KenticoCloud.ContentManagement.Models.Items;
using System.Threading.Tasks;

namespace GithubService.Services.Clients
{
    public class KenticoCloudClient : IKenticoCloudClient
    {
        private readonly ContentManagementClient _contentManagementClient;
        private readonly IKenticoCloudInternalClient _internalApiClient;

        public KenticoCloudClient(string projectId, string contentManagementApiKey, string internalApiKey)
        {
            var options = new ContentManagementOptions
            {
                ApiKey = contentManagementApiKey,
                ProjectId = projectId
            };
            _contentManagementClient = new ContentManagementClient(options);
            _internalApiClient = new KenticoCloudInternalClient(projectId, internalApiKey);
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
            => await _internalApiClient.CreateNewVersionOfDefaultVariantAsync(contentItem.Id);
    }
}
