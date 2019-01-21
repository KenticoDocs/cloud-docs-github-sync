using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;
using System;
using System.Net;
using System.Threading.Tasks;

namespace GithubService.Services
{
    public class KenticoCloudService : IKenticoCloudService
    {
        private readonly IKenticoCloudClient _kcClient;
        private readonly ICodeSamplesConverter _codeConverter;

        public KenticoCloudService(IKenticoCloudClient kcClient, ICodeSamplesConverter codeConverter)
        {
            _kcClient = kcClient;
            _codeConverter = codeConverter;
        }

        public async Task<CodeBlock> UpsertCodeBlockAsync(CodenameCodeSamples codeSamples)
        {
            var codeBlock = _codeConverter.ConvertToCodeBlock(codeSamples);
            var contentItem = await EnsureCodeBlockItemAsync(codeSamples.Codename);

            return await EnsureCodeBlockVariantAsync(contentItem, codeBlock);
        }

        public bool DeleteCodeSampleItem(CodenameCodeSamples sample)
        {
            throw new NotImplementedException();
        }

        private async Task<ContentItemModel> EnsureCodeBlockItemAsync(string codename)
        {
            try
            {
                // Try to get the content item from KC using codename
                return await _kcClient.GetContentItemAsync(codename);
            }
            catch (ContentManagementException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;

                // Content item doesn't exist in KC -> create it
                var codeBlockItem = new ContentItemCreateModel
                {
                    Type = ContentTypeIdentifier.ByCodename("code_block"),
                    Name = codename
                };
                return await _kcClient.CreateContentItemAsync(codeBlockItem);
            }
        }

        private async Task<CodeBlock> EnsureCodeBlockVariantAsync(ContentItemModel contentItem, CodeBlock codeBlock)
        {
            try
            {
                // Try to update the content variant in KC
                return await _kcClient.UpsertCodeBlockVariantAsync(contentItem, codeBlock);
            }
            catch (ContentManagementException exception)
            {
                if (!exception.Message.Contains("Cannot update published content"))
                    throw;

                // The variant seems to be published -> create new version in KC
                await _kcClient.CreateNewVersionOfDefaultVariantAsync(contentItem);

                // The content variant should be updated correctly now
                return await _kcClient.UpsertCodeBlockVariantAsync(contentItem, codeBlock);
            }
        }
    }
}
