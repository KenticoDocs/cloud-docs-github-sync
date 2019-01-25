using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;
using System.Net;
using System.Threading.Tasks;
using GithubService.Models;

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

        public async Task<CodeSamples> UpsertCodeFragmentAsync(CodeFragment codeFragment)
        {
            var contentItem = await EnsureCodeSamplesItemAsync(codeFragment.Codename);
            var codeSamples = UpdateValuesInCodeSamples(new CodeSamples(), codeFragment.Language, codeFragment.Content);

            return await EnsureCodeSamplesVariantAsync(contentItem, codeSamples);
        }

        public async Task<CodeSamples> UpsertCodeFragmentsAsync(CodenameCodeFragments codenameCodeFragments)
        {
            var codeSamples = _codeConverter.ConvertToCodeSamples(codenameCodeFragments);
            var contentItem = await EnsureCodeSamplesItemAsync(codenameCodeFragments.Codename);

            return await EnsureCodeSamplesVariantAsync(contentItem, codeSamples);
        }

        public async Task RemoveCodeFragmentAsync(CodeFragment codeFragment)
        {
            // Try to get the content item from KC using codename
            var contentItem = await _kcClient.GetContentItemAsync(codeFragment.Codename);
            var codeSamples = await _kcClient.GetCodeSamplesVariantAsync(contentItem);
            codeSamples = UpdateValuesInCodeSamples(codeSamples, codeFragment.Language, codeFragment.Content);
            
            await EnsureCodeSamplesVariantAsync(contentItem, codeSamples);

            if (IsReadyToUnpublish(codeSamples))
            {
                // TODO: sent to new workflow step, once it is defined
            }
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

        private CodeSamples UpdateValuesInCodeSamples(CodeSamples codeSamples, CodeFragmentLanguage language, string codeContent)
        {
            switch (language)
            {
                case CodeFragmentLanguage.CUrl:
                    codeSamples.Curl = codeContent;
                    break;
                case CodeFragmentLanguage.CSharp:
                    codeSamples.CSharp = codeContent;
                    break;
                case CodeFragmentLanguage.JavaScript:
                    codeSamples.JavaScript = codeContent;
                    break;
                case CodeFragmentLanguage.TypeScript:
                    codeSamples.TypeScript = codeContent;
                    break;
                case CodeFragmentLanguage.Java:
                    codeSamples.Java = codeContent;
                    break;
                case CodeFragmentLanguage.JavaRx:
                    codeSamples.JavaRx = codeContent;
                    break;
                case CodeFragmentLanguage.Swift:
                    codeSamples.Swift = codeContent;
                    break;
                case CodeFragmentLanguage.Ruby:
                    codeSamples.Ruby = codeContent;
                    break;
                case CodeFragmentLanguage.PHP:
                    codeSamples.PHP = codeContent;
                    break;
            }

            return codeSamples;
        }
    }
}
