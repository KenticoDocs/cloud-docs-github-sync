using GithubService.Models.CodeSamples;
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

namespace GithubService.Services.Tests
{
    public class KenticoCloudServiceTests
    {
        private static readonly Guid CodeSampleGuid = Guid.NewGuid();

        private static readonly CodenameCodeSamples CodeSamples =
            new CodenameCodeSamples
            {
                Codename = CodeSampleGuid.ToString(),
                CodeSamples = new Dictionary<CodeLanguage, string>
                {
                    { CodeLanguage.CSharp, "var the_answer = 42;" }
                }
            };

        private static readonly CodeBlock CodeBlock = new CodeBlock
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
            _codeConverter.ConvertToCodeBlock(CodeSamples)
                .Returns(CodeBlock);
        }

        [Test]
        public void UpsertCodeBlockAsync_NoItem_CreatesCodeBlock()
        {
            // Arrange
            var kcClientException = new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename).
                Throws(kcClientException);
            kcClient.CreateContentItemAsync(Arg.Any<ContentItemCreateModel>())
                .Returns(ContentItem);
            kcClient.GetCodeBlockVariantAsync(Arg.Any<ContentItemModel>())
                .Throws(kcClientException);
            kcClient.UpsertCodeBlockVariantAsync(ContentItem, CodeBlock)
                .Returns(CodeBlock);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = kcService.UpsertCodeBlockAsync(CodeSamples).Result;

            // Assert
            Assert.AreEqual(CodeBlock, result);
        }

        [Test]
        public void UpsertCodeBlockAsync_ExistingUnpublishedItem_UpdatesCodeBlock()
        {
            // Arrange
            var kcClientException = new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename)
                .Returns(ContentItem);
            kcClient.GetCodeBlockVariantAsync(Arg.Any<ContentItemModel>())
                .Returns(CodeBlock);
            kcClient.UpsertCodeBlockVariantAsync(ContentItem, CodeBlock)
                .Returns(CodeBlock);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = kcService.UpsertCodeBlockAsync(CodeSamples).Result;

            // Assert
            Assert.AreEqual(CodeBlock, result);
        }

        [Test]
        public void UpsertCodeBlockAsync_ExistingPublishedItem_CreatesNewVersion()
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
            kcClient.GetContentItemAsync(CodeSamples.Codename)
                .Returns(ContentItem);
            kcClient.GetCodeBlockVariantAsync(Arg.Any<ContentItemModel>())
                .Returns(CodeBlock);
            kcClient.When(x => x.CreateNewVersionOfDefaultVariantAsync(Arg.Any<ContentItemModel>()))
                .Do(_ => createNewVersionCalled++);
            kcClient.UpsertCodeBlockVariantAsync(ContentItem, CodeBlock)
                .Throws(kcClientException).AndDoes(_ => upsertCalled++);

            // Act
            try
            {
                var kcService = new KenticoCloudService(kcClient, _codeConverter);
                var result = kcService.UpsertCodeBlockAsync(CodeSamples).Result;
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

        [Test]
        public void UpsertCodeBlockAsync_UnknownExceptionInGetContentItemAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename)
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodeBlockAsync(CodeSamples));
        }

        [Test]
        public void UpsertCodeBlockAsync_UnknownExceptionInGetCodeBlockVariantAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename)
                .Returns(ContentItem);
            kcClient.GetCodeBlockVariantAsync(Arg.Any<ContentItemModel>())
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodeBlockAsync(CodeSamples));
        }

        [Test]
        public void UpsertCodeBlockAsync_UnknownExceptionInUpsertCodeBlockVariantAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename)
                .Returns(ContentItem);
            kcClient.GetCodeBlockVariantAsync(Arg.Any<ContentItemModel>())
                .Returns(CodeBlock);
            kcClient.UpsertCodeBlockVariantAsync(ContentItem, CodeBlock)
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodeBlockAsync(CodeSamples));
        }
    }
}
