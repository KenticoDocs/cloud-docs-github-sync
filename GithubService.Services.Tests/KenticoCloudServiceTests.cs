using System;
using System.Net;
using System.Net.Http;
using GithubService.Models.CodeSamples;
using GithubService.Models.KenticoCloud;
using GithubService.Services.Interfaces;
using KenticoCloud.ContentManagement.Exceptions;
using KenticoCloud.ContentManagement.Models.Items;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace GithubService.Services.Tests
{
    public class KenticoCloudServiceTests
    {
        private static readonly Guid CodeSampleGuid = Guid.NewGuid();

        private static readonly CodenameCodeSamples CodeSamples =
            new CodenameCodeSamples
            {
                Codename = CodeSampleGuid.ToString(),
            };

        private static readonly CodeBlock CodeBlock = new CodeBlock
        {
            Identifier = CodeSamples.Codename
        };

        private static readonly ContentItemModel ContentItem = new ContentItemModel
        {
            Id = CodeSampleGuid
        };

        private IKenticoCloudInternalClient _kcClientInternal;
        private ICodeSamplesConverter _codeConverter;

        [SetUp]
        public void SetUp()
        {
            _kcClientInternal = Substitute.For<IKenticoCloudInternalClient>();
            _codeConverter = Substitute.For<ICodeSamplesConverter>();
            _codeConverter.ConvertToCodeBlock(CodeSamples).Returns(CodeBlock);
        }

        [Test]
        public void CreateCodeSampleItemAsync_NewItem_CreatesVariant()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename).Throws(kcClientException);
            kcClient.CreateContentItemAsync(Arg.Any<ContentItemCreateModel>())
                .Returns(ContentItem);
            kcClient.UpsertContentItemVariantAsync(CodeBlock, ContentItem)
                .Returns(CodeBlock);
            
            // Act
            var kcService = new KenticoCloudService(kcClient, _kcClientInternal, _codeConverter);
            var result = kcService.CreateCodeSampleItemAsync(CodeSamples).Result;

            // Assert
            Assert.AreEqual(CodeBlock, result);
        }

        [Test]
        public void CreateCodeSampleItemAsync_UnpublishedExistingItem_UpdatesVariant()
        {
            // Arrange
            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename).Returns(ContentItem);
            kcClient.UpsertContentItemVariantAsync(CodeBlock, ContentItem).Returns(CodeBlock);

            // Act
            var kcService = new KenticoCloudService(kcClient, _kcClientInternal, _codeConverter);
            var result = kcService.CreateCodeSampleItemAsync(CodeSamples).Result;

            // Assert
            Assert.AreEqual(CodeBlock, result);
        }

        [Test]
        public void CreateCodeSampleItemAsync_PublishedExistingItem_CreatesNewVersion()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Cannot update published content"
            };
            var kcClientException = new ContentManagementException(responseMessage, string.Empty);
            var called = 0;
            
            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename).Returns(ContentItem);
            kcClient
                .UpsertContentItemVariantAsync(CodeBlock, ContentItem)
                .Throws(kcClientException).AndDoes(_ => called++);

            // Act
            try
            {
                var kcService = new KenticoCloudService(kcClient, _kcClientInternal, _codeConverter);
                var result = kcService.CreateCodeSampleItemAsync(CodeSamples).Result;
            }
            catch (Exception)
            {
                // do nothing as we need to check if the method was called twice,
                // we can't change return value for mocked method
            }

            // Assert
            Assert.AreEqual(2, called);
        }

        [Test]
        public void CreateCodeSampleItemAsync_PublishedExistingItem_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeSamples.Codename).Returns(ContentItem);
            kcClient.UpsertContentItemVariantAsync(CodeBlock, ContentItem).Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _kcClientInternal, _codeConverter);
            
            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.CreateCodeSampleItemAsync(CodeSamples));
        }
    }
}
