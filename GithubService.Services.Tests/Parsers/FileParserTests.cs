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

        [TestCase("js/file.js", CodeFragmentPlatform.JavaScript, CodeFragmentLanguage.JavaScript)]
        [TestCase("php/file.css", CodeFragmentPlatform.Php, CodeFragmentLanguage.Css)]
        public void ParseContent_ParsesFileWithOneCodeSample(string filePath, string platform, string language)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile = $"{comment} DocSection: hello\nanything \n{comment} EndDocSection";
            var expectedOutput = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                {
                    new CodeFragment
                    {
                        Language = language,
                        Identifier = "hello",
                        Content = "anything",
                        Platform = platform,
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [TestCase("js/file.css", CodeFragmentPlatform.JavaScript, CodeFragmentLanguage.Css)]
        [TestCase("php/file.php", CodeFragmentPlatform.Php, CodeFragmentLanguage.Php)]
        public void ParseContent_ThrowsWhenMultipleCodeSamplesWithSameIdentifierInOneFile(string filePath, string platform, string language)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile =
$@"{comment} DocSection: create-integer
int i = 1000;
{comment} EndDocSection
{comment} DocSection: create-integer
int i = 10;
int j = 14;
{comment} EndDocSection
  ";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent(filePath, sampleFile));
        }

        [TestCase("js/file.css", CodeFragmentPlatform.JavaScript, CodeFragmentLanguage.Css)]
        [TestCase("php/file.php", CodeFragmentPlatform.Php, CodeFragmentLanguage.Php)]
        [TestCase("android/file.java", CodeFragmentPlatform.Android, CodeFragmentLanguage.Java)]
        [TestCase("ruby/file.rb", CodeFragmentPlatform.Ruby, CodeFragmentLanguage.Ruby)]
        [TestCase("ios/file.swift", CodeFragmentPlatform.iOS, CodeFragmentLanguage.Swift)]
        [TestCase("net/file.cs", CodeFragmentPlatform.Net, CodeFragmentLanguage.CSharp)]
        [TestCase("ts/file.ts", CodeFragmentPlatform.TypeScript, CodeFragmentLanguage.TypeScript)]
        [TestCase("js/file.html", CodeFragmentPlatform.JavaScript, CodeFragmentLanguage.HTML)]
        public void ParseContent_ParsesFileWithMultipleCodeSamples(string filePath, string platform, string language)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile =
$@"{comment} DocSection: hello_world
console.log(""Hello Kentico Cloud"");
{comment} EndDocSection
{comment} DocSection: create-integer-long-codename123
int abcd = 12345;
{comment} EndDocSection
{comment} DocSection: create-integer
int i = 1000;
{comment} EndDocSection
{comment} DocSection: create-integer-variable
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
                        Identifier = "hello_world",
                        Language = language,
                        Content = "console.log(\"Hello Kentico Cloud\");",
                        Platform = platform
                    },
                    new CodeFragment
                    {
                        Identifier = "create-integer-long-codename123",
                        Language = language,
                        Content = "int abcd = 12345;",
                        Platform = platform
                    },
                    new CodeFragment
                    {
                        Identifier = "create-integer",
                        Language = language,
                        Content = "int i = 1000;",
                        Platform = platform
                    },
                    new CodeFragment
                    {
                        Identifier = "create-integer-variable",
                        Language = language,
                        Content = $"import com.kenticocloud.delivery;{Environment.NewLine}DeliveryClient client = new DeliveryClient(\"<YOUR_PROJECT_ID>\", \"<YOUR_PREVIEW_API_KEY>\");",
                        Platform = platform
                    }
                },
                FilePath = filePath
            };

            var actualOutput = _parser.ParseContent(filePath, sampleFile);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodeSampleFileComparer());
        }

        [TestCase("js/file.css", CodeFragmentPlatform.JavaScript, CodeFragmentLanguage.Css)]
        [TestCase("php/file.php", CodeFragmentPlatform.Php, CodeFragmentLanguage.Php)]
        [TestCase("android/file.java", CodeFragmentPlatform.Android, CodeFragmentLanguage.Java)]
        [TestCase("ruby/file.rb", CodeFragmentPlatform.Ruby, CodeFragmentLanguage.Ruby)]
        [TestCase("ios/file.swift", CodeFragmentPlatform.iOS, CodeFragmentLanguage.Swift)]
        [TestCase("net/file.cs", CodeFragmentPlatform.Net, CodeFragmentLanguage.CSharp)]
        [TestCase("ts/file.ts", CodeFragmentPlatform.TypeScript, CodeFragmentLanguage.TypeScript)]
        [TestCase("js/file.html", CodeFragmentPlatform.JavaScript, CodeFragmentLanguage.HTML)]
        public void ParseContent_ParsesCodeSampleWithSpecialCharacters(string filePath, string platform, string language)
        {
            var comment = language.GetCommentPrefix();
            var sampleFile =
$@"   {comment} DocSection: special_
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
                        Identifier = "special_",
                        Content = $";0123456789.*?()<>@/'{Environment.NewLine}{Environment.NewLine}\t\a\b\f\v\\|%$&:+-;~`!#^_{{}}[], //",
                        Platform = platform
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
                        Language = CodeFragmentLanguage.Java,
                        Identifier = "content_unpublishing",
                        Content = ComplexCodeSample,
                        Platform = CodeFragmentPlatform.Java
                    }
                },
                FilePath = "java/unpublishing.java"
            };

            var actualOutput = _parser.ParseContent("java/unpublishing.java", ComplexSampleFile);

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
            => Assert.Throws<ArgumentException>(() => _parser.ParseContent(filePath, "some content"));

        [Test]
        public void ParseContent_NestedSamples_ThrowsArgumentException()
        {
            const string fileContent =
@"// DocSection: hello-world
console.log(""Hello Kentico Cloud, from Javascript"");
// DocSection: create-integer
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
@"// DocSection: hello-world
console.log(""Hello Kentico Cloud, from Javascript"");
// DocSection: create-integer
int i = 10;
int j = 14;
// EndDocSection
// EndDocSection
";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", fileContent));
        }


        [Test]
        public void ParseContent_ThrowsIfMarkingIsNotProperlyClosed()
        {
            const string sampleFile = "// DocSection: hello\nanything";

            Assert.Throws<ArgumentException>(() => _parser.ParseContent("js/file.js", sampleFile));
        }

        private const string ComplexSampleFile =
@"// DocSection: content_unpublishing

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
