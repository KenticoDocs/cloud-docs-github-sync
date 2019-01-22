using System.Collections.Generic;
using System.Linq;
using GithubService.Models.CodeSamples;
using GithubService.Services.Converters;
using GithubService.Services.Interfaces;
using GithubService.Services.Tests.Utils;
using NUnit.Framework;

namespace GithubService.Services.Tests.Converters
{
    internal class CodeSamplesConverterTests
    {
        private readonly ICodeSamplesConverter _codeSamplesConverter = new CodeSamplesConverter();

        [Test]
        public void ConvertToCodenameCodeSamples_OnEmptyCodeSampleFiles_ReturnsEmptyList()
        {
            var codeSampleFile = new CodeSampleFile();
            var codeSampleFilesList = new List<CodeSampleFile> {codeSampleFile};
            var expectedOutput = new List<CodenameCodeSamples>();

            var actualOutput =
                _codeSamplesConverter.ConvertToCodenameCodeSamples(new List<CodeSampleFile>(codeSampleFilesList));

            Assert.That(expectedOutput, Is.EqualTo(actualOutput.ToList()));
        }

        [Test]
        public void ConvertToCodenameCodeSamples_HandlesMultipleCodeSamplesInOneFile()
        {
            var codeSampleFile = new CodeSampleFile
            {
                CodeSamples = new List<CodeSample>
                {
                    new CodeSample
                    {
                        Codename = "hello-world",
                        Content = "console.log('Hello Kentico Cloud');",
                        Language = CodeLanguage.Javascript
                    },
                    new CodeSample
                    {
                        Codename = "one-two-three",
                        Content = "int i += 3",
                        Language = CodeLanguage.Javascript
                    },
                    new CodeSample
                    {
                        Codename = "let_s_begin",
                        Content =
                            "const client = new DeliveryClient({ projectId: 'a0a9d198-e604-007a-50c9-fecbb46046d1' });",
                        Language = CodeLanguage.Javascript
                    },
                    new CodeSample
                    {
                        Codename = "byebye",
                        Content = "export default ArticleListing;",
                        Language = CodeLanguage.Javascript
                    }
                }
            };
            var codeSampleFilesList = new List<CodeSampleFile> {codeSampleFile};
            var expectedOutput = new List<CodenameCodeSamples>
            {
                new CodenameCodeSamples
                {
                    Codename = "hello-world",
                    CodeSamples = new Dictionary<CodeLanguage, string>
                    {
                        {CodeLanguage.Javascript, "console.log('Hello Kentico Cloud');"}
                    }
                },
                new CodenameCodeSamples
                {
                    Codename = "one-two-three",
                    CodeSamples = new Dictionary<CodeLanguage, string>
                    {
                        {CodeLanguage.Javascript, "int i += 3"}
                    }
                },
                new CodenameCodeSamples
                {
                    Codename = "let_s_begin",
                    CodeSamples = new Dictionary<CodeLanguage, string>
                    {
                        {
                            CodeLanguage.Javascript,
                            "const client = new DeliveryClient({ projectId: 'a0a9d198-e604-007a-50c9-fecbb46046d1' });"
                        }
                    }
                },
                new CodenameCodeSamples
                {
                    Codename = "byebye",
                    CodeSamples = new Dictionary<CodeLanguage, string>
                    {
                        {CodeLanguage.Javascript, "export default ArticleListing;"}
                    }
                }
            };

            var actualOutput =
                _codeSamplesConverter.ConvertToCodenameCodeSamples(new List<CodeSampleFile>(codeSampleFilesList));

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodenameCodeSamplesComparer());
        }

        [Test]
        public void ConvertToCodenameCodeSamples_ComposesSamplesFromMultipleFilesIntoOneObject()
        {
            var codeSampleFilesList = new List<CodeSampleFile>
            {
                new CodeSampleFile
                {
                    CodeSamples = new List<CodeSample>
                    {
                        new CodeSample
                        {
                            Codename = "hello-world",
                            Content = "console.log('Hello Kentico Cloud');",
                            Language = CodeLanguage.Javascript
                        }
                    }
                },
                new CodeSampleFile
                {
                    CodeSamples = new List<CodeSample>
                    {
                        new CodeSample
                        {
                            Codename = "hello-world",
                            Content = "Console.WriteLine(\"Hello World!\");",
                            Language = CodeLanguage.CSharp
                        }
                    }
                },
                new CodeSampleFile
                {
                    CodeSamples = new List<CodeSample>
                    {
                        new CodeSample
                        {
                            Codename = "hello-world",
                            Content = "print(\"Hello, World!\")",
                            Language = CodeLanguage.Python
                        }
                    }
                },
                new CodeSampleFile
                {
                    CodeSamples = new List<CodeSample>
                    {
                        new CodeSample
                        {
                            Codename = "hello-world",
                            Content = "System.out.println(\"Hello, World\");",
                            Language = CodeLanguage.Java
                        }
                    }
                }
            };
            var expectedOutput = new List<CodenameCodeSamples>
            {
                new CodenameCodeSamples
                {
                    Codename = "hello-world",
                    CodeSamples = new Dictionary<CodeLanguage, string>
                    {
                        {CodeLanguage.Javascript, "console.log('Hello Kentico Cloud');"},
                        {CodeLanguage.CSharp, "Console.WriteLine(\"Hello World!\");"},
                        {CodeLanguage.Python, "print(\"Hello, World!\")"},
                        {CodeLanguage.Java, "System.out.println(\"Hello, World\");"}
                    }
                }
            };

            var actualOutput = _codeSamplesConverter.ConvertToCodenameCodeSamples(new List<CodeSampleFile>(codeSampleFilesList));

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodenameCodeSamplesComparer());
        }
    }
}
