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
            ContentItemModel contentItem;
            var codeBlockUpdate = _codeConverter.ConvertToCodeBlock(codeSamples);

            contentItem = await EnsureCodeBlockItemAsync(codeSamples);

            return await EnsureCodeBlockVariantAsync(contentItem, codeBlockUpdate);
        }

        public bool DeleteCodeSampleItem(CodenameCodeSamples sample)
        {
            throw new NotImplementedException();
        }

        private async Task<ContentItemModel> EnsureCodeBlockItemAsync(CodenameCodeSamples codeSamples)
        {
            try
            {
                // Try to get the content item from KC using codename
                return await _kcClient.GetContentItemAsync(codeSamples.Codename);
            }
            catch (ContentManagementException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;

                // Content item doesn't exist in KC -> create it
                var codeBlockItem = new ContentItemCreateModel
                {
                    Type = ContentTypeIdentifier.ByCodename("code_block"),
                    Name = codeSamples.Codename
                };
                return await _kcClient.CreateContentItemAsync(codeBlockItem);
            }
        }

        private async Task<CodeBlock> EnsureCodeBlockVariantAsync(ContentItemModel contentItem, CodeBlock codeBlockUpdate)
        {
            CodeBlock codeBlock;

            try
            {
                codeBlock = await _kcClient.GetCodeBlockVariantAsync(contentItem);
            }
            catch (ContentManagementException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;

                // Content variant doesn't exist -> create in KC
                return await _kcClient.UpsertCodeBlockVariantAsync(contentItem, codeBlockUpdate);
            }

            try
            {
                // Content variant already exists -> try to update in KC
                MergeCodeBlocks(codeBlock, codeBlockUpdate);
                return await _kcClient.UpsertCodeBlockVariantAsync(contentItem, codeBlock);
            }
            catch (ContentManagementException exception)
            {
                if (!exception.Message.Contains("Cannot update published content"))
                    throw;

                // The variant seems to be published -> create new version in KC
                await _kcClient.CreateNewVersionOfDefaultVariantAsync(contentItem);

                // the variant should be updated correctly now
                return await _kcClient.UpsertCodeBlockVariantAsync(contentItem, codeBlock);
            }
        }

        private void MergeCodeBlocks(CodeBlock oldCodeBlock, CodeBlock newCodeblock)
        {
            if (newCodeblock.CSharp != null)
                oldCodeBlock.CSharp = newCodeblock.CSharp;

            if (newCodeblock.Curl != null)
                oldCodeBlock.Curl = newCodeblock.Curl;

            if (newCodeblock.Java != null)
                oldCodeBlock.Java = newCodeblock.Java;

            if (newCodeblock.Javarx != null)
                oldCodeBlock.Javarx = newCodeblock.Javarx;

            if (newCodeblock.Js != null)
                oldCodeBlock.Js = newCodeblock.Js;

            if (newCodeblock.Python != null)
                oldCodeBlock.Python = newCodeblock.Python;

            if (newCodeblock.Ruby != null)
                oldCodeBlock.Ruby = newCodeblock.Ruby;

            if (newCodeblock.Swift != null)
                oldCodeBlock.Swift = newCodeblock.Swift;

            if (newCodeblock.Ts != null)
                oldCodeBlock.Ts = newCodeblock.Ts;
        }
    }
}
