using GithubService.Models;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;
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

        public async Task<CodeSamples> UpsertCodeFragmentsAsync(CodenameCodeFragments fragments)
        {
            var contentItem = await EnsureItemAsync(fragments.Codename, "code_samples");

            return await EnsureCodeSamplesVariantAsync(contentItem, fragments);
        }

        public async Task<CodeSample> UpsertCodeFragmentAsync(CodeFragment fragment)
        {
            var contentItem = await EnsureItemAsync(fragment.Codename, "code_sample");

            return await EnsureCodeSampleVariantAsync(contentItem, fragment);
        }

        public async Task<CodeSamples> RemoveCodeFragmentsAsync(CodenameCodeFragments fragments)
        {
            var contentItem = await _kcClient.GetContentItemAsync(fragments.Codename);
            var codeSamples = await _kcClient.GetVariantAsync<CodeSamples>(contentItem);

            foreach (var codeFragment in fragments.CodeFragments)
            {
                switch (codeFragment.Language)
                {
                    case CodeFragmentLanguage.Curl:
                        codeSamples.Curl = string.Empty;
                        break;
                    case CodeFragmentLanguage.Net:
                        codeSamples.CSharp = string.Empty;
                        break;
                    case CodeFragmentLanguage.JavaScript:
                        codeSamples.JavaScript = string.Empty;
                        break;
                    case CodeFragmentLanguage.TypeScript:
                        codeSamples.TypeScript = string.Empty;
                        break;
                    case CodeFragmentLanguage.Java:
                        codeSamples.Java = string.Empty;
                        break;
                    case CodeFragmentLanguage.JavaRx:
                        codeSamples.JavaRx = string.Empty;
                        break;
                    case CodeFragmentLanguage.Php:
                        codeSamples.PHP = string.Empty;
                        break;
                    case CodeFragmentLanguage.Swift:
                        codeSamples.Swift = string.Empty;
                        break;
                    case CodeFragmentLanguage.Ruby:
                        codeSamples.Ruby = string.Empty;
                        break;
                }
            }

            var updatedCodeSamples = await EnsureVariantAsync(contentItem, codeSamples);

            if (IsReadyToUnpublish(updatedCodeSamples))
            {
                // TODO: Send to new workflow step, once it's defined
            }

            return updatedCodeSamples;
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

        private async Task<CodeSamples> EnsureCodeSamplesVariantAsync(ContentItemModel contentItem, CodenameCodeFragments fragments)
        {
            CodeSamples existingCodeSamples;
            CodeSamples newCodeSamples;

            try
            {
                // Try to get the variant from KC
                existingCodeSamples = await _kcClient.GetVariantAsync<CodeSamples>(contentItem);
            }
            catch (ContentManagementException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;

                // Content variant doesn't exist -> create in KC
                newCodeSamples = _codeConverter.ConvertToCodeSamples(fragments);
                return await _kcClient.UpsertVariantAsync(contentItem, newCodeSamples);
            }

            newCodeSamples = UpdateValuesInCodeSamples(existingCodeSamples, fragments);

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

        private bool IsReadyToUnpublish(CodeSamples codeSamples)
            => string.IsNullOrEmpty(codeSamples.Curl) &&
               string.IsNullOrEmpty(codeSamples.CSharp) &&
               string.IsNullOrEmpty(codeSamples.Java) &&
               string.IsNullOrEmpty(codeSamples.JavaRx) &&
               string.IsNullOrEmpty(codeSamples.JavaScript) &&
               string.IsNullOrEmpty(codeSamples.PHP) &&
               string.IsNullOrEmpty(codeSamples.Ruby) &&
               string.IsNullOrEmpty(codeSamples.TypeScript);

        private CodeSamples UpdateValuesInCodeSamples(CodeSamples codeSamples, CodenameCodeFragments fragments)
        {
            foreach (var codeFragment in fragments.CodeFragments)
            {
                switch (codeFragment.Language)
                {
                    case CodeFragmentLanguage.Curl:
                        codeSamples.Curl = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.Net:
                        codeSamples.CSharp = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.JavaScript:
                        codeSamples.JavaScript = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.TypeScript:
                        codeSamples.TypeScript = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.Java:
                        codeSamples.Java = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.JavaRx:
                        codeSamples.JavaRx = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.Php:
                        codeSamples.PHP = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.Swift:
                        codeSamples.Swift = codeFragment.Content;
                        break;
                    case CodeFragmentLanguage.Ruby:
                        codeSamples.Ruby = codeFragment.Content;
                        break;
                }
            }

            return codeSamples;
        }
    }
}
