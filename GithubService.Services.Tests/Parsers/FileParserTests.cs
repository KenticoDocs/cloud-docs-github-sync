using System;
using System.Collections.Generic;
using GithubService.Models;
using GithubService.Services.Interfaces;
using GithubService.Services.Parsers;
using GithubService.Services.Tests.Utils;
using NUnit.Framework;

namespace GithubService.Services.Tests.Parsers
{
    internal class FileParserTests
    {
        private readonly IFileParser _parser = new FileParser();

        [TestCase(CodeFragmentLanguage.JavaScript, "js/file.js")]
        [TestCase(CodeFragmentLanguage.PHP, "php/file.php")]
        public void ParseContent_ParsesFileWithOneCodeSample(CodeFragmentLanguage language, string filePath)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile = $"{comment} DocSection: multiple_hello\nanything \n{comment} EndDocSection";
            var expectedOutput = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                {
                    new CodeFragment
                    {
                        Language = language,
                        Codename = "hello",
                        Type = CodeFragmentType.Multiple,
                        Content = "anything"
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [TestCase(CodeFragmentLanguage.JavaScript, "js/file.js")]
        [TestCase(CodeFragmentLanguage.PHP, "php/file.php")]
        public void ParseContent_ParsesFileWithMultipleCodeSamples(CodeFragmentLanguage language, string filePath)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile =
$@"{comment} DocSection: multiple_hello_world
console.log(""Hello Kentico Cloud"");
{comment} EndDocSection
{comment} DocSection: single_create-integer-long-codename123
int abcd = 12345;
{comment} EndDocSection
{comment} DocSection: single_create-integer
int i = 1000;
{comment} EndDocSection
{comment} DocSection: multiple_create-integer
int i = 10;
int j = 14;
{comment} EndDocSection
{comment} DocSection: single_create-integer-variable
import com.kenticocloud.delivery;
DeliveryClient client = new DeliveryClient(""<YOUR_PROJECT_ID>"", ""<YOUR_PREVIEW_API_KEY>"");
{comment} EndDocSection
  ";

            var expectedOutput = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                {
                    new CodeFragment
                    {
                        Language = language,
                        Codename = "hello_world",
                        Type = CodeFragmentType.Multiple,
                        Content = "console.log(\"Hello Kentico Cloud\");"
                    },
                    new CodeFragment
                    {
                        Language = language,
                        Codename = "create-integer-long-codename123",
                        Type = CodeFragmentType.Single,
                        Content = "int abcd = 12345;"
                    },
                    new CodeFragment
                    {
                        Language = language,
                        Codename = "create-integer",
                        Type = CodeFragmentType.Single,
                        Content = "int i = 1000;"
                    },
                    new CodeFragment
                    {
                        Language = language,
                        Codename = "create-integer",
                        Type = CodeFragmentType.Multiple,
                        Content = $"int i = 10;{Environment.NewLine}int j = 14;"
                    },
                    new CodeFragment
                    {
                        Language = language,
                        Codename = "create-integer-variable",
                        Type = CodeFragmentType.Single,
                        Content = $"import com.kenticocloud.delivery;{Environment.NewLine}DeliveryClient client = new DeliveryClient(\"<YOUR_PROJECT_ID>\", \"<YOUR_PREVIEW_API_KEY>\");"
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [TestCase(CodeFragmentLanguage.JavaScript, "js/file.js")]
        [TestCase(CodeFragmentLanguage.PHP, "php/file.php")]
        public void ParseContent_ParsesCodeSampleWithSpecialCharacters(CodeFragmentLanguage language, string filePath)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile =
$@"   {comment} DocSection: multiple_special_
;0123456789.*?()<>@/'

	\|%$&:+-;~`!#^_{{}}[], //
{comment} EndDocSection";

            var expectedOutput = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                {
                    new CodeFragment
                    {
                        Language = language,
                        Codename = "special_",
                        Type = CodeFragmentType.Multiple,
                        Content = $";0123456789.*?()<>@/'{Environment.NewLine}{Environment.NewLine}\t\a\b\f\v\\|%$&:+-;~`!#^_{{}}[], //"
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [Test]
        public void ParseContent_ParsesComplexCodeSample()
        {
            var expectedOutput = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                {
                    new CodeFragment
                    {
                        Language = CodeFragmentLanguage.JavaRx,
                        Codename = "content_unpublishing",
                        Type = CodeFragmentType.Multiple,
                        Content = ComplexCodeSample,
                    }
                },
                FilePath = "javarx/unpublishing.java"
            };

            var actualOutput = _parser.ParseContent("javarx/unpublishing.java", ComplexSampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [TestCase("js/file.js")]
        [TestCase("python/file.py")]
        public void ParseContent_ReturnsEmptyObjectOnEmptyFile(string filePath)
        {
            const string sampleFile = "";
            var expectedOutput = new CodeFile
            {
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [TestCase("js/file.js")]
        [TestCase("python/file.py")]
        public void ParseContent_ReturnsEmptyObjectOnFileWithoutMarkedCodeSamples(string filePath)
        {
            const string sampleFile = "any text";
            var expectedOutput = new CodeFile
            {
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [Test]
        public void ParseContent_ReturnsEmptyObjectOnFileWithoutSpecifiedLanguage()
        {
            const string filePath = "README.md";
            const string sampleFile = "# kentico-cloud-docs-samples\nKentico Cloud documentation - code samples";
            var expectedOutput = new CodeFile
            {
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
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
@"// DocSection: multiple_hello-world
console.log(""Hello Kentico Cloud, from Javascript"");
// DocSection: multiple_create-integer
int i = 10;
int j = 14;
// EndDocSection
// EndDocSection
";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", fileContent));
        }

        [Test]
        public void ParseContent_IntersectedSamples_ThrowsArgumentException()
        {
            const string fileContent =
@"// DocSection: multiple_hello-world
console.log(""Hello Kentico Cloud, from Javascript"");
// DocSection: multiple_create-integer
int i = 10;
int j = 14;
// EndDocSection
// EndDocSection
";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", fileContent));
        }

        [Test]
        public void ParseContent_InvalidSampleType_ThrowsArgumentException()
        {
            const string fileContent =
@"// DocSection: invalid_hello-world
console.log(""Hello Kentico Cloud, from Javascript"");
int i = 10;
int j = 14;
// EndDocSection
";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", fileContent));
        }

        [Test]
        public void ParseContent_ThrowsIfMarkingIsNotProperlyClosed()
        {
            const string sampleFile = "// DocSection: multiple_hello\nanything";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", sampleFile));
        }

        private const string ComplexSampleFile =
@"// DocSection: multiple_content_unpublishing

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

// EndDocSection
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
