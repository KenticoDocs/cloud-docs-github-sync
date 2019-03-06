using GithubService.Models;
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
using System.Threading.Tasks;
using System.Collections;
using GithubService.Services.Tests.Utils;

namespace GithubService.Services.Tests
{
    public class KenticoCloudServiceTests
    {
        private static readonly Guid CodeSampleGuid = Guid.NewGuid();

        private static readonly CodeFragment CodeFragment = new CodeFragment
        {
            Codename = CodeSampleGuid.ToString(),
            Content = "var the_answer = 42;",
            Language = CodeFragmentLanguage.Net
        };

        private static readonly CodenameCodeFragments CodenameCodeFragments =
            new CodenameCodeFragments
            {
                Codename = CodeFragment.Codename,
                CodeFragments = new List<CodeFragment>
                {
                    CodeFragment
                }
            };

        private static readonly CodeSample CodeSample = new CodeSample
        {
            Code = CodeFragment.Content,
            ProgrammingLanguage = new[] {TaxonomyTermIdentifier.ByCodename(CodeFragment.Language)}
        };

        private static readonly CodeSamples CodeSamples = new CodeSamples
        {
            Samples = new[] {ContentItemIdentifier.ByCodename(CodeFragment.Codename)}
        };

        private static readonly ContentItemModel ContentItem = new ContentItemModel
        {
            Id = CodeSampleGuid
        };

        private ICodeConverter _codeConverter;

        [SetUp]
        public void SetUp()
        {
            _codeConverter = Substitute.For<ICodeConverter>();
        }

        [Test]
        public async Task UpsertCodeFragmentAsync_NoItem_CreatesCodeSample()
        {
            // Arrange
            var notFoundException = new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeFragment.Codename).
                Throws(notFoundException);
            kcClient.CreateContentItemAsync(Arg.Any<ContentItemCreateModel>())
                .Returns(ContentItem);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Any<CodeSample>())
                .Returns(CodeSample);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = await kcService.UpsertCodeFragmentAsync(CodeFragment);

            // Assert
            Assert.AreEqual(CodeSample, result);
        }

        [Test]
        public async Task UpsertCodeFragmentAsync_ExistingUnpublishedItem_UpdatesCodeSample()
        {
            // Arrange
            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Returns(ContentItem);
            kcClient.GetVariantAsync<CodeSample>(Arg.Any<ContentItemModel>())
                .Returns(new CodeSample());
            kcClient.UpsertVariantAsync(Arg.Any<ContentItemModel>(), Arg.Any<CodeSample>())
                .Returns(CodeSample);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = await kcService.UpsertCodeFragmentAsync(CodeFragment);

            // Assert
            Assert.AreEqual(CodeSample, result);
        }

        [Test]
        public async Task UpsertCodeFragmentAsync_ExistingPublishedItem_CreatesNewVersion()
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
            kcClient.GetVariantAsync<CodeSample>(Arg.Any<ContentItemModel>())
                .Returns(new CodeSample());
            kcClient.When(x => x.CreateNewVersionOfDefaultVariantAsync(Arg.Any<ContentItemModel>()))
                .Do(_ => createNewVersionCalled++);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Any<CodeSample>())
                .Throws(kcClientException).AndDoes(_ => upsertCalled++);

            // Act
            try
            {
                var kcService = new KenticoCloudService(kcClient, _codeConverter);
                await kcService.UpsertCodeFragmentAsync(CodeFragment);
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
        public void UpsertCodeFragmentAsync_UnknownExceptionInGetContentItemAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeFragment.Codename)
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodeFragmentAsync(CodeFragment));
        }

        [Test]
        public void UpsertCodeFragmentAsync_UnknownExceptionInUpsertCodeSampleVariantAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeFragment.Codename)
                .Returns(ContentItem);
            kcClient.GetVariantAsync<CodeSample>(Arg.Any<ContentItemModel>())
                .Returns(CodeSample);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Any<CodeSample>())
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodeFragmentAsync(CodeFragment));
        }

        [Test]
        public async Task RemoveCodeFragmentAsync_CodeSampleVariantExists_RemovesCode()
        {
            // Arrange
            var upserted = false;

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodeFragment.Codename)
                .Returns(ContentItem);
            kcClient.GetVariantAsync<CodeSample>(ContentItem)
                .Returns(CodeSample);
            kcClient.UpsertVariantAsync(Arg.Any<ContentItemModel>(), Arg.Any<CodeSample>())
                .Returns(CodeSample);
            kcClient.When(client => client.UpsertVariantAsync(ContentItem, Arg.Is<CodeSample>(sample => sample.Code == string.Empty)))
                .Do(_ => upserted = true);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            await kcService.RemoveCodeFragmentAsync(CodeFragment);

            // Assert
            Assert.IsTrue(upserted);
        }

        [Test]
        public async Task RemoveCodeFragmentAsync_CodeSampleVariantIsPublished_CreatesNewVariantWithRemovedCode()
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
            kcClient.GetVariantAsync<CodeSample>(ContentItem)
                .Returns(CodeSample);
            kcClient.When(client => client.CreateNewVersionOfDefaultVariantAsync(Arg.Any<ContentItemModel>()))
                .Do(_ => createNewVersionCalled++);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Is<CodeSample>(sample => sample.Code == string.Empty))
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

        [Test]
        public async Task UpsertCodenameCodeFragmentsAsync_NoItem_CreatesCodenameCodeSamples()
        {
            // Arrange
            var notFoundException = new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Throws(notFoundException);
            kcClient.CreateContentItemAsync(Arg.Any<ContentItemCreateModel>())
                .Returns(ContentItem);
            kcClient.GetVariantAsync<CodeSamples>(ContentItem)
                .Throws(notFoundException);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Any<CodeSamples>())
                .Returns(parameters => parameters.ArgAt<CodeSamples>(1));

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = await kcService.UpsertCodenameCodeFragmentsAsync(CodenameCodeFragments);

            // Assert
            Assert.That(CodeSamples, Is.EqualTo(result).UsingCodeSamplesComparer());
        }

        [Test]
        public async Task UpsertCodenameCodeFragmentsAsync_NoLinkedItemsInCloud_AddLinkedCodeSample()
        {
            // Arrange
            var notFoundException = new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Returns(ContentItem);
            kcClient.GetVariantAsync<CodeSamples>(ContentItem)
                .Throws(notFoundException);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Any<CodeSamples>())
                .Returns(parameters => parameters.ArgAt<CodeSamples>(1));

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = await kcService.UpsertCodenameCodeFragmentsAsync(CodenameCodeFragments);

            // Assert
            Assert.That(CodeSamples, Is.EqualTo(result).UsingCodeSamplesComparer());
        }

        [Test]
        public async Task UpsertCodenameCodeFragmentsAsync_LinkedItemsInCloud_UpdateLinkedCodeSample()
        {
            // Arrange
            var notFoundException = new ContentManagementException(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty);
            var linkedCodeSampleCodeName = "linked_code_sample";

            var linkedCodeSample = new CodeSample
            {
                Code = "console.log(Hello World);",
                ProgrammingLanguage = new[] {TaxonomyTermIdentifier.ByCodename(CodeFragmentLanguage.JavaScript)},
            };

            var cloudCodeSamples = new CodeSamples
            {
                Samples = new [] {ContentItemIdentifier.ByCodename(linkedCodeSampleCodeName) }
            };

            var expectedCodeSamples = new CodeSamples
            {
                Samples = new[] { ContentItemIdentifier.ByCodename(CodeFragment.Codename), ContentItemIdentifier.ByCodename(linkedCodeSampleCodeName) }
            };

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Returns(ContentItem);
            kcClient.GetVariantAsync<CodeSamples>(ContentItem)
                .Returns(cloudCodeSamples);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Any<CodeSamples>())
                .Returns(parameters => parameters.ArgAt<CodeSamples>(1));

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);
            var result = await kcService.UpsertCodenameCodeFragmentsAsync(CodenameCodeFragments);

            // Assert
            Assert.That(expectedCodeSamples, Is.EqualTo(result).UsingCodeSamplesComparer());
        }

        [Test]
        public void UpsertCodenameCodeFragmentsAsync_UnknownExceptionInUpsertVariantAsyncMethod_RethrowsException()
        {
            // Arrange
            var kcClientException =
                new ContentManagementException(new HttpResponseMessage(HttpStatusCode.InternalServerError), string.Empty);

            var kcClient = Substitute.For<IKenticoCloudClient>();
            kcClient.GetContentItemAsync(CodenameCodeFragments.Codename)
                .Returns(ContentItem);
            kcClient.GetVariantAsync<CodeSamples>(Arg.Any<ContentItemModel>())
                .Returns(CodeSamples);
            kcClient.UpsertVariantAsync(ContentItem, Arg.Any<CodeSamples>())
                .Throws(kcClientException);

            // Act
            var kcService = new KenticoCloudService(kcClient, _codeConverter);

            // Assert
            Assert.ThrowsAsync<ContentManagementException>(() => kcService.UpsertCodenameCodeFragmentsAsync(CodenameCodeFragments));
        }
    }
}
