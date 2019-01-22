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
        private readonly ICodeSamplesConverter _codeConverter;

        public KenticoCloudService(IKenticoCloudClient kcClient, ICodeSamplesConverter codeConverter)
        {
            _kcClient = kcClient;
            _codeConverter = codeConverter;
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
            var codeBlockFromKc = await _kcClient.GetCodeSamplesVariantAsync(contentItem);

            // remove the code
            switch (codeFragment.Language)
            {
                case CodeFragmentLanguage.CUrl:
                    codeBlockFromKc.Curl = string.Empty;
                    break;
                case CodeFragmentLanguage.CSharp:
                    codeBlockFromKc.CSharp = string.Empty;
                    break;
                case CodeFragmentLanguage.JavaScript:
                    codeBlockFromKc.JavaScript = string.Empty;
                    break;
                case CodeFragmentLanguage.TypeScript:
                    codeBlockFromKc.TypeScript = string.Empty;
                    break;
                case CodeFragmentLanguage.Java:
                    codeBlockFromKc.Java = string.Empty;
                    break;
                case CodeFragmentLanguage.JavaRx:
                    codeBlockFromKc.JavaRx = string.Empty;
                    break;
                case CodeFragmentLanguage.Swift:
                    codeBlockFromKc.Swift = string.Empty;
                    break;
                case CodeFragmentLanguage.Ruby:
                    codeBlockFromKc.Ruby = string.Empty;
                    break;
                case CodeFragmentLanguage.PHP:
                    codeBlockFromKc.PHP = string.Empty;
                    break;
            }

            await EnsureCodeSamplesVariantAsync(contentItem, codeBlockFromKc);

            if (IsReadyToUnpublish(codeBlockFromKc))
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


    }
}
