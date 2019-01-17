using System;
using System.Net;
using System.Threading.Tasks;
using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;

namespace GithubService.Services
{
    public class KenticoCloudService: IKenticoCloudService
    {
        private readonly IKenticoCloudClient _kcClient;
        private readonly IKenticoCloudInternalClient _internalKcClient;
        private readonly ICodeSamplesConverter _codeConverter;

        public KenticoCloudService(IKenticoCloudClient kcClient, IKenticoCloudInternalClient internalKcClient, ICodeSamplesConverter codeConverter)
        {
            _kcClient = kcClient;
            _internalKcClient = internalKcClient;
            _codeConverter = codeConverter;
        }

        public async Task<CodeBlock> CreateCodeSampleItemAsync(CodenameCodeSamples sample)
        {
            ContentItemModel contentItem;
            var codeBlock = _codeConverter.ConvertToCodeBlock(sample);

            try
            {
                contentItem = await _kcClient.GetContentItemAsync(sample.Codename);
            }
            catch (ContentManagementException exception)
            {
                // if not found, we first need to create content item
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;

                var newContentItem = new ContentItemCreateModel
                {
                    Type = ContentTypeIdentifier.ByCodename("code_block"),
                    Name = sample.Codename
                };
                contentItem = await _kcClient.CreateContentItemAsync(newContentItem);
            }

            try
            {
                return await _kcClient.UpsertContentItemVariantAsync(codeBlock, contentItem);
            }
            catch (ContentManagementException exception)
            {
                // we need to figure out if the item was not changed due to published variant
                if (!exception.Message.Contains("Cannot update published content"))
                    throw;

                await _internalKcClient.CreateNewVersionOfDefaultVariantAsync(contentItem.Id);

                // the variant should be updated correctly now
                return await _kcClient.UpsertContentItemVariantAsync(codeBlock, contentItem);
            }
        }

        public bool UpdateCodeSampleItem(CodenameCodeSamples sample)
        {
            throw new NotImplementedException();
        }

        public bool DeleteCodeSampleItem(CodenameCodeSamples sample)
        {
            throw new NotImplementedException();
        }
    }
}
