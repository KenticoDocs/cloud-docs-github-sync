using GithubService.Models;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GithubService.Services
{
    public class KenticoCloudService : IKenticoCloudService
    {
        private readonly IKenticoCloudClient _kcClient;
        private readonly ICodeConverter _codeConverter;

        public KenticoCloudService(IKenticoCloudClient kcClient, ICodeConverter codeConverter)
        {
            _kcClient = kcClient;
            _codeConverter = codeConverter;
        }

        public async Task<CodeSample> UpsertCodeFragmentAsync(CodeFragment fragment)
        {
            var contentItem = await EnsureItemAsync(fragment.Codename, "code_sample");

            return await EnsureCodeSampleVariantAsync(contentItem, fragment);
        }

        public async Task<CodeSamples> UpsertCodenameCodeFragmentsAsync(CodenameCodeFragments fragments)
        {
            var contentItem = await EnsureItemAsync(fragments.Codename, "code_samples");
            
            return await EnsureCodeSamplesVariantAsync(contentItem, fragments.CodeFragments);
        }

        public async Task<CodeSample> RemoveCodeFragmentAsync(CodeFragment fragment)
        { 
            var contentItem = await _kcClient.GetContentItemAsync(fragment.Codename);
            var codeSample = await _kcClient.GetVariantAsync<CodeSample>(contentItem);
            codeSample.Code = string.Empty;

            var updatedCodeSample = await EnsureVariantAsync(contentItem, codeSample);

            // TODO: Send to new workflow step, once it's defined	

            return updatedCodeSample;
        }

        private async Task<ContentItemModel> EnsureItemAsync(string codename, string contentType)
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
                var codeSamplesItem = new ContentItemCreateModel
                {
                    Type = ContentTypeIdentifier.ByCodename(contentType),
                    Name = _codeConverter.ConvertCodenameToItemName(codename)
                };
                return await _kcClient.CreateContentItemAsync(codeSamplesItem);
            }
        }

        private async Task<CodeSamples> EnsureCodeSamplesVariantAsync(ContentItemModel contentItem, List<CodeFragment> fragments)
        {
            var cloudCodeSamples = new List<ContentItemIdentifier>();
            var linkedCodeSamples = new List<ContentItemIdentifier>();

            try
            {
                var retrievedCodeSample = await _kcClient.GetVariantAsync<CodeSamples>(contentItem);
                cloudCodeSamples.AddRange(retrievedCodeSample.Samples);
            }
            catch (ContentManagementException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }

            foreach(var fragment in fragments)
            {
                linkedCodeSamples.Add(ContentItemIdentifier.ByCodename(fragment.Codename));
            }

            linkedCodeSamples.AddRange(cloudCodeSamples);

            var newCodeSamples = new CodeSamples
            {
                Samples = linkedCodeSamples
            };

            return await EnsureVariantAsync(contentItem, newCodeSamples);
        }

        private async Task<CodeSample> EnsureCodeSampleVariantAsync(ContentItemModel contentItem, CodeFragment fragment)
        {
            var newCodeSample = new CodeSample
            {
                ProgrammingLanguage = new [] { TaxonomyTermIdentifier.ByCodename(fragment.Language) },
                Code = fragment.Content
            };

            return await EnsureVariantAsync(contentItem, newCodeSample);
        }

        private async Task<T> EnsureVariantAsync<T>(ContentItemModel contentItem, T variant) where T : new()
        {
            try
            {
                // Try to update the content variant in KC
                return await _kcClient.UpsertVariantAsync(contentItem, variant);
            }
            catch (ContentManagementException exception)
            {
                if (!exception.Message.Contains("Cannot update published content"))
                    throw;

                // The variant seems to be published -> create new version in KC
                await _kcClient.CreateNewVersionOfDefaultVariantAsync(contentItem);

                // The content variant should be updated correctly now
                return await _kcClient.UpsertVariantAsync(contentItem, variant);
            }
        }
    }
}
