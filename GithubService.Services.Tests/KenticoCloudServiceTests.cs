using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GithubService.Models;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;

namespace GithubService.Services.Tests
{
    public class KenticoCloudServiceTests
    {
        private ICodeSamplesConverter _codeConverter;

        private static readonly Guid CodeSampleGuid = Guid.NewGuid();
        private static readonly CodeFragment CodeFragment = new CodeFragment
        {
            Codename = CodeSampleGuid.ToString(),
            Content = "var the_answer = 42;",
            Language = CodeFragmentLanguage.CSharp
        };

        private static readonly CodenameCodeFragments CodenameCodeFragments =
            new CodenameCodeFragments
            {
                Codename = CodeFragment.Codename,
                CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                {
                    { CodeFragment.Language, CodeFragment.Content }
                }
            };

        private static readonly CodeSamples CodeSamples = new CodeSamples
        {
            CSharp = CodeFragment.Content
        };

        private static readonly ContentItemModel ContentItem = new ContentItemModel
        {
            Id = CodeSampleGuid
        };

        [SetUp]
        public void SetUp()
        {
            _codeConverter = Substitute.For<ICodeSamplesConverter>();
            _codeConverter.ConvertToCodeSamples(CodenameCodeFragments)
                .Returns(CodeSamples);
        }

        [Test]
        public void UpsertCodeSamplesAsync_NoItem_CreatesCodeSamples()
        {
            // Arrange
            var kcClientException = new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename).
                Throws(kcClientException);
            kcClient.CreateContentItemAsync(Arg.Any<ContentItemCreateModel>())
                .Returns(ContentItem);
            kcClient.UpsertCodeSamplesVariantAsync(ContentItem, CodeSamples)
                .Returns(CodeSamples);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = kcService.UpsertCodeFragmentsAsync(CodenameCodeFragments).Result;

            // Assert
            Assert.AreEqual(CodeSamples, result);
        }

        [Test]
        public void UpsertCodeSamplesAsync_ExistingUnpublishedItem_UpdatesCodeSamples()
        {
            // Arrange
            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Returns(ContentItem);
            kcClient.UpsertCodeSamplesVariantAsync(ContentItem, CodeSamples)
                .Returns(CodeSamples);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = kcService.UpsertCodeFragmentsAsync(CodenameCodeFragments).Result;

            // Assert
            Assert.AreEqual(CodeSamples, result);
        }

        [Test]
        public void UpsertCodeSamplesAsync_ExistingPublishedItem_CreatesNewVersion()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Cannot update published content"
            };
            var kcClientException = new ContentManagementException(responseMessage, string.Empty);
            var upsertCalled = 0;
            var createNewVersionCalled = 0;

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Returns(ContentItem);
            kcClient.When(x => x.CreateNewVersionOfDefaultVariantAsync(Arg.Any<ContentItemModel>()))
                .Do(_ => createNewVersionCalled++);
            kcClient.UpsertCodeSamplesVariantAsync(ContentItem, CodeSamples)
                .Throws(kcClientException).AndDoes(_ => upsertCalled++);

            // Act
            try
            {
                var kcService = new KenticoCloudService(kcClient, _codeConverter);
                var result = kcService.UpsertCodeFragmentsAsync(CodenameCodeFragments).Result;
            }
            catch (Exception)
            {
                // Do nothing as we need to check if the method was called twice
                // This try-catch block is needed because we cannot change the kcClient
                // mock during the UpsertCodeSamplesAsync method execution
            }

            // Assert
            Assert.AreEqual(1, createNewVersionCalled);
            Assert.AreEqual(2, upsertCalled);
        }

        [Test]
        public void UpsertCodeSamplesAsync_UnknownExceptionInGetContentItemAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodeFragmentsAsync(CodenameCodeFragments));
        }

        [Test]
        public void UpsertCodeSamplesAsync_UnknownExceptionInUpsertCodeSamplesVariantAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Returns(ContentItem);
            kcClient.UpsertCodeSamplesVariantAsync(ContentItem, CodeSamples)
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodeFragmentsAsync(CodenameCodeFragments));
        }

        [Test]
        public async Task RemoveCodeBlockSampleAsync_CodeBlockExists_RemovesCode()
        {
            // Arrange
            var upserted = false;

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeFragment.Codename)
                .Returns(ContentItem);
            kcClient.GetCodeSamplesVariantAsync(ContentItem)
                .Returns(CodeSamples);
            kcClient.When(client => client.UpsertCodeSamplesVariantAsync(ContentItem, Arg.Is<CodeSamples>(sample => sample.CSharp == string.Empty)))
                .Do(_ => upserted = true);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            await kcService.RemoveCodeFragmentAsync(CodeFragment);

            // Assert
            Assert.IsTrue(upserted);
        }

        [Test]
        public async Task RemoveCodeBlockSampleAsync_CodeBlockIsPublished_CreatesNewVariantWithRemovedCode()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Cannot update published content"
            };
            var kcClientException = new ContentManagementException(responseMessage, string.Empty);
            var upsertCalled = 0;
            var createNewVersionCalled = 0;

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeFragment.Codename)
                .Returns(ContentItem);
            kcClient.GetCodeSamplesVariantAsync(ContentItem)
                .Returns(CodeSamples);
            kcClient.When(client => client.CreateNewVersionOfDefaultVariantAsync(Arg.Any<ContentItemModel>()))
                .Do(_ => createNewVersionCalled++);
            kcClient.UpsertCodeSamplesVariantAsync(ContentItem, Arg.Is<CodeSamples>(sample => sample.CSharp == string.Empty))
                .Throws(kcClientException).AndDoes(_ => upsertCalled++);

            // Act
            try
            {
                var kcService = new KenticoCloudService(kcClient, _codeConverter);
                await kcService.RemoveCodeFragmentAsync(CodeFragment);
            }
            catch (Exception)
            {
                // Do nothing as we need to check if the method was called twice
                // This try-catch block is needed because we cannot change the kcClient
                // mock during the UpsertCodeBlockAsync method execution
            }

            // Assert
            Assert.AreEqual(1, createNewVersionCalled);
            Assert.AreEqual(2, upsertCalled);
        }
    }
}
