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
            var contentItem = await EnsureCodeSamplesItemAsync(fragments.Codename);

            return await EnsureCodeSamplesVariantAsync(contentItem, fragments);
        }

        public async Task<CodeSamples> RemoveCodeFragmentsAsync(CodenameCodeFragments fragments)
        {
            var contentItem = await _kcClient.GetContentItemAsync(fragments.Codename);
            var codeSamples = await _kcClient.GetCodeSamplesVariantAsync(contentItem);

            foreach (var codeFragment in fragments.CodeFragments)
            {
                switch (codeFragment.Key)
                {
                    case CodeFragmentLanguage.CUrl:
                        codeSamples.Curl = string.Empty;
                        break;
                    case CodeFragmentLanguage.CSharp:
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
                    case CodeFragmentLanguage.PHP:
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

            var updatedCodeSamples = await EnsureCodeSamplesVariantAsync(contentItem, codeSamples);

            if (IsReadyToUnpublish(updatedCodeSamples))
            {
                // TODO: Send to new workflow step, once it's defined
            }

            return updatedCodeSamples;
        }

        private async Task<ContentItemModel> EnsureCodeSamplesItemAsync(string codename)
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
                    Type = ContentTypeIdentifier.ByCodename("code_samples"),
                    Name = codename
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
                existingCodeSamples = await _kcClient.GetCodeSamplesVariantAsync(contentItem);
            }
            catch (ContentManagementException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;

                // Content variant doesn't exist -> create in KC
                newCodeSamples = _codeConverter.ConvertToCodeSamples(fragments);
                return await _kcClient.UpsertCodeSamplesVariantAsync(contentItem, newCodeSamples);
            }

            newCodeSamples = UpdateValuesInCodeSamples(existingCodeSamples, fragments);

            return await EnsureCodeSamplesVariantAsync(contentItem, newCodeSamples);
        }

        private async Task<CodeSamples> EnsureCodeSamplesVariantAsync(ContentItemModel contentItem, CodeSamples codeSamples)
        {
            try
            {
                // Try to update the content variant in KC
                return await _kcClient.UpsertCodeSamplesVariantAsync(contentItem, codeSamples);
            }
            catch (ContentManagementException exception)
            {
                if (!exception.Message.Contains("Cannot update published content"))
                    throw;

                // The variant seems to be published -> create new version in KC
                await _kcClient.CreateNewVersionOfDefaultVariantAsync(contentItem);

                // The content variant should be updated correctly now
                return await _kcClient.UpsertCodeSamplesVariantAsync(contentItem, codeSamples);
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
                switch (codeFragment.Key)
                {
                    case CodeFragmentLanguage.CUrl:
                        codeSamples.Curl = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.CSharp:
                        codeSamples.CSharp = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.JavaScript:
                        codeSamples.JavaScript = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.TypeScript:
                        codeSamples.TypeScript = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.Java:
                        codeSamples.Java = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.JavaRx:
                        codeSamples.JavaRx = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.PHP:
                        codeSamples.PHP = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.Swift:
                        codeSamples.Swift = codeFragment.Value;
                        break;
                    case CodeFragmentLanguage.Ruby:
                        codeSamples.Ruby = codeFragment.Value;
                        break;
                }
            }

            return codeSamples;
        }
    }
}
