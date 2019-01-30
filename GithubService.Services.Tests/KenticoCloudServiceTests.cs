using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using GithubService.Models;

namespace GithubService.Services.Tests
{
    public class KenticoCloudServiceTests
    {
        private static readonly Guid CodeSampleGuid = Guid.NewGuid();

        private static readonly CodenameCodeFragments CodenameCodeFragments =
            new CodenameCodeFragments
            {
                Codename = CodeSampleGuid.ToString(),
                CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                {
                    { CodeFragmentLanguage.CSharp, "var the_answer = 42;" }
                }
            };

        private static readonly CodeSamples CodeSamples = new CodeSamples
        {
            CSharp = "var the_answer = 42;"
        };

        private static readonly ContentItemModel ContentItem = new ContentItemModel
        {
            Id = CodeSampleGuid
        };

        private ICodeSamplesConverter _codeConverter;

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
    }
}
