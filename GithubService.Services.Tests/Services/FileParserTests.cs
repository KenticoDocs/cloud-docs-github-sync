using GithubService.Models.CodeSamples;
using GithubService.Services.Interfaces;
using GithubService.Services.Services;
using GithubService.Services.Tests.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GithubService.Services.Tests.Services
{
    internal class FileParserTests
    {
        private readonly IFileParser _parser = new FileParser();

        [TestCase(CodeLanguage.Javascript, "js/file.js")]
        [TestCase(CodeLanguage.Python, "python/file.py")]
        public void ParseContent_ParsesFileWithOneCodeSample(CodeLanguage language, string filePath)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile = $"{comment} start::hello\nanything \n{comment} end::hello";
            var expectedOutput = new CodeSampleFile
            {
                CodeSamples = new List<CodeSample>
                {
                    new CodeSample
                    {
                        Language = language,
                        Codename = "hello",
                        Content = "anything"
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodeSampleFileComparer());
        }

        [TestCase(CodeLanguage.Javascript, "js/file.js")]
        [TestCase(CodeLanguage.Python, "python/file.py")]
        public void ParseContent_ParsesFileWithMultipleCodeSamples(CodeLanguage language, string filePath)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile =
$@"{comment} start::hello_world
console.log(""Hello Kentico Cloud"");
{comment} end::hello_world
{comment} start::create-integer
int i = 10;
int j = 14;
{comment} end::create-integer
{comment} start::make_coffee
import com.kenticocloud.delivery;
DeliveryClient client = new DeliveryClient(""<YOUR_PROJECT_ID>"", ""<YOUR_PREVIEW_API_KEY>"");
{comment} end::make_coffee
  ";

            var expectedOutput = new CodeSampleFile
            {
                CodeSamples = new List<CodeSample>
                {
                    new CodeSample
                    {
                        Language = language,
                        Codename = "hello_world",
                        Content = "console.log(\"Hello Kentico Cloud\");"
                    },
                    new CodeSample
                    {
                        Language = language,
                        Codename = "create-integer",
                        Content = $"int i = 10;{Environment.NewLine}int j = 14;"
                    },
                    new CodeSample
                    {
                        Language = language,
                        Codename = "make_coffee",
                        Content = $"import com.kenticocloud.delivery;{Environment.NewLine}DeliveryClient client = new DeliveryClient(\"<YOUR_PROJECT_ID>\", \"<YOUR_PREVIEW_API_KEY>\");"
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodeSampleFileComparer());
        }

        [TestCase(CodeLanguage.Javascript, "js/file.js")]
        [TestCase(CodeLanguage.Python, "python/file.py")]
        public void ParseContent_ParsesCodeSampleWithSpecialCharacters(CodeLanguage language, string filePath)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile =
$@"   {comment} start::_special_
;0123456789.*?()<>@/'

	\|%$&:+-;~`!#^_{{}}[], //
{comment} end::_special_";

            var expectedOutput = new CodeSampleFile
            {
                CodeSamples = new List<CodeSample>
                {
                    new CodeSample
                    {
                        Language = language,
                        Codename = "_special_",
                        Content = $";0123456789.*?()<>@/'{Environment.NewLine}{Environment.NewLine}\t\a\b\f\v\\|%$&:+-;~`!#^_{{}}[], //"
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodeSampleFileComparer());
        }

        [Test]
        public void ParseContent_ParsesComplexCodeSample()
        {
            var expectedOutput = new CodeSampleFile
            {
                CodeSamples = new List<CodeSample>
                {
                    new CodeSample
                    {
                        Language = CodeLanguage.JavaRx,
                        Codename = "content_unpublishing",
                        Content = ComplexCodeSample,
                    }
                },
                FilePath = "javarx/unpublishing.java"
            };

            var actualOutput = _parser.ParseContent("javarx/unpublishing.java", ComplexSampleFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodeSampleFileComparer());
        }

        [TestCase("js/file.js")]
        [TestCase("python/file.py")]
        public void ParseContent_ReturnsEmptyObjectOnFileWithoutMarkedCodeSamples(string filePath)
        {
            const string sampleFile = "any text";
            var expectedOutput = new CodeSampleFile
            {
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodeSampleFileComparer());
        }

        [Test]
        public void ParseContent_ReturnsEmptyObjectOnFileWithoutSpecifiedLanguage()
        {
            const string filePath = "README.md";
            const string sampleFile = "# kentico-cloud-docs-samples\nKentico Cloud documentation - code samples";
            var expectedOutput = new CodeSampleFile
            {
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodeSampleFileComparer());
        }

        [TestCase(null)]
        [TestCase("")]
        public void ParseContent_InvalidFilePath_ThrowsArgumentException(string filePath)
        {
            Assert.Throws<ArgumentException>(() => _parser.ParseContent(filePath, "some content"));
        }

        [Test]
        public void ParseContent_NestedSamples_ThrowsArgumentException()
        {
            const string fileContent =
@"// start::hello-world
console.log(""Hello Kentico Cloud, from Javascript"");
// start::create-integer
int i = 10;
int j = 14;
// end::create-integer
// end::hello-world
";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", fileContent));
        }

        [Test]
        public void ParseContent_IntersectedSamples_ThrowsArgumentException()
        {
            const string fileContent =
@"// start::hello-world
console.log(""Hello Kentico Cloud, from Javascript"");
// start::create-integer
int i = 10;
int j = 14;
// end::hello-world
// end::create-integer
";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", fileContent));
        }

        [Test]
        public void ParseContent_ThrowsIfCodenamesDoNotMatch()
        {
            const string sampleFile = " \n// start::hello\nanything\n// end::byebye\n";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", sampleFile));
        }

        [Test]
        public void ParseContent_ThrowsIfMarkingIsNotProperlyClosed()
        {
            const string sampleFile = "// start::hello\nanything";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", sampleFile));
        }

        private const string ComplexSampleFile =
@"// start::content_unpublishing

import com.kenticocloud.delivery_core.*;
    import com.kenticocloud.delivery_rx.*;

    import io.reactivex.Observer;
    import io.reactivex.disposables.Disposable;
    import io.reactivex.functions.Function;

    Date now = new Date();

    // Prepares an array to hold strongly-typed models
    List<TypeResolver<?>> typeResolvers = new ArrayList<>();

    // Registers the type resolvers
    typeResolvers.add(new TypeResolver<>(Article.TYPE, new Function<Void, Article>() {
    @Override
    public Article apply(Void input)
    {
        return new Article();
    }
}));

// Prepares the DeliveryService configuration object
IDeliveryConfig config = DeliveryConfig.newConfig(projectId)
    .withTypeResolvers(typeResolvers);

// Initializes a DeliveryService for Java projects
IDeliveryService deliveryService = new DeliveryService(config);

// Gets all articles using a simple request
List<Article> articles = deliveryService.< Article > items()
        .equalsFilter('system.type', 'article')
        .get()
        .getItems();

List<Article> publishedItems = new ArrayList<>();

for (Article article : articles) {
    if (
        article.getPublishUntil() == null || article.getPublishUntil().after(now)) {
        publishedItems.add(article);
    }
}

// Gets all articles using RxJava2
deliveryService.<Article>items()
        .equalsFilter('system.type', 'article')
        .getObservable()
        .subscribe(new Observer<DeliveryItemListingResponse<Article>>() {
            @Override
            public void onSubscribe(Disposable d) {}

// end::content_unpublishing
";

        private const string ComplexCodeSample =
@"import com.kenticocloud.delivery_core.*;
    import com.kenticocloud.delivery_rx.*;

    import io.reactivex.Observer;
    import io.reactivex.disposables.Disposable;
    import io.reactivex.functions.Function;

    Date now = new Date();

    // Prepares an array to hold strongly-typed models
    List<TypeResolver<?>> typeResolvers = new ArrayList<>();

    // Registers the type resolvers
    typeResolvers.add(new TypeResolver<>(Article.TYPE, new Function<Void, Article>() {
    @Override
    public Article apply(Void input)
    {
        return new Article();
    }
}));

// Prepares the DeliveryService configuration object
IDeliveryConfig config = DeliveryConfig.newConfig(projectId)
    .withTypeResolvers(typeResolvers);

// Initializes a DeliveryService for Java projects
IDeliveryService deliveryService = new DeliveryService(config);

// Gets all articles using a simple request
List<Article> articles = deliveryService.< Article > items()
        .equalsFilter('system.type', 'article')
        .get()
        .getItems();

List<Article> publishedItems = new ArrayList<>();

for (Article article : articles) {
    if (
        article.getPublishUntil() == null || article.getPublishUntil().after(now)) {
        publishedItems.add(article);
    }
}

// Gets all articles using RxJava2
deliveryService.<Article>items()
        .equalsFilter('system.type', 'article')
        .getObservable()
        .subscribe(new Observer<DeliveryItemListingResponse<Article>>() {
            @Override
            public void onSubscribe(Disposable d) {}";
    }
}
