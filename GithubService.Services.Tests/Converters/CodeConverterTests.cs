using System.Collections.Generic;
using System.Linq;
using GithubService.Models;
using GithubService.Services.Converters;
using GithubService.Services.Interfaces;
using GithubService.Services.Tests.Utils;
using NUnit.Framework;

namespace GithubService.Services.Tests.Converters
{
    internal class CodeConverterTests
    {
        private readonly ICodeConverter _codeSamplesConverter = new CodeConverter();

        [Test]
        public void ConvertToCodenameCodeFragments_OnEmptyCodeFiles_ReturnsEmptyList()
        {
            var codeFile = new CodeFile();
            var codeFilesList = new List<CodeFile> {codeFile};
            var expectedOutput = new List<CodenameCodeFragments>();

            var actualOutput =
                _codeSamplesConverter.ConvertToCodenameCodeFragments(new List<CodeFile>(codeFilesList));

            Assert.That(expectedOutput, Is.EqualTo(actualOutput.ToList()));
        }

        [Test]
        public void ConvertToCodenameCodeFragments_HandlesMultipleCodeFragmentsInOneFile()
        {
            var codeFile = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                {
                    new CodeFragment
                    {
                        Codename = "hello-world",
                        Content = "console.log('Hello Kentico Cloud');",
                        Language = CodeFragmentLanguage.JavaScript
                    },
                    new CodeFragment
                    {
                        Codename = "one-two-three",
                        Content = "int i += 3",
                        Language = CodeFragmentLanguage.JavaScript
                    },
                    new CodeFragment
                    {
                        Codename = "let_s_begin",
                        Content =
                            "const client = new DeliveryClient({ projectId: 'a0a9d198-e604-007a-50c9-fecbb46046d1' });",
                        Language = CodeFragmentLanguage.JavaScript
                    },
                    new CodeFragment
                    {
                        Codename = "byebye",
                        Content = "export default ArticleListing;",
                        Language = CodeFragmentLanguage.JavaScript
                    }
                }
            };
            var codeFilesList = new List<CodeFile> {codeFile};
            var expectedOutput = new List<CodenameCodeFragments>
            {
                new CodenameCodeFragments
                {
                    Codename = "hello-world",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {CodeFragmentLanguage.JavaScript, "console.log('Hello Kentico Cloud');"}
                    }
                },
                new CodenameCodeFragments
                {
                    Codename = "one-two-three",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {CodeFragmentLanguage.JavaScript, "int i += 3"}
                    }
                },
                new CodenameCodeFragments
                {
                    Codename = "let_s_begin",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {
                            CodeFragmentLanguage.JavaScript,
                            "const client = new DeliveryClient({ projectId: 'a0a9d198-e604-007a-50c9-fecbb46046d1' });"
                        }
                    }
                },
                new CodenameCodeFragments
                {
                    Codename = "byebye",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {CodeFragmentLanguage.JavaScript, "export default ArticleListing;"}
                    }
                }
            };

            var actualOutput =
                _codeSamplesConverter.ConvertToCodenameCodeFragments(new List<CodeFile>(codeFilesList));

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodenameCodeSamplesComparer());
        }

        [Test]
        public void ConvertToCodenameCodeFragments_ComposesFragmentsFromMultipleFilesIntoOneObject()
        {
            var codeFilesList = new List<CodeFile>
            {
                new CodeFile
                {
                    CodeFragments = new List<CodeFragment>
                    {
                        new CodeFragment
                        {
                            Codename = "hello-world",
                            Content = "console.log('Hello Kentico Cloud');",
                            Language = CodeFragmentLanguage.JavaScript
                        }
                    }
                },
                new CodeFile
                {
                    CodeFragments = new List<CodeFragment>
                    {
                        new CodeFragment
                        {
                            Codename = "hello-world",
                            Content = "Console.WriteLine(\"Hello World!\");",
                            Language = CodeFragmentLanguage.CSharp
                        }
                    }
                },
                new CodeFile
                {
                    CodeFragments = new List<CodeFragment>
                    {
                        new CodeFragment
                        {
                            Codename = "hello-world",
                            Content = "print(\"Hello, World!\")",
                            Language = CodeFragmentLanguage.PHP
                        }
                    }
                },
                new CodeFile
                {
                    CodeFragments = new List<CodeFragment>
                    {
                        new CodeFragment
                        {
                            Codename = "hello-world",
                            Content = "System.out.println(\"Hello, World\");",
                            Language = CodeFragmentLanguage.Java
                        }
                    }
                }
            };
            var expectedOutput = new List<CodenameCodeFragments>
            {
                new CodenameCodeFragments
                {
                    Codename = "hello-world",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {CodeFragmentLanguage.JavaScript, "console.log('Hello Kentico Cloud');"},
                        {CodeFragmentLanguage.CSharp, "Console.WriteLine(\"Hello World!\");"},
                        {CodeFragmentLanguage.PHP, "print(\"Hello, World!\")"},
                        {CodeFragmentLanguage.Java, "System.out.println(\"Hello, World\");"}
                    }
                }
            };

            var actualOutput = _codeSamplesConverter.ConvertToCodenameCodeFragments(new List<CodeFile>(codeFilesList));

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodenameCodeSamplesComparer());
        }

        [Test]
        public void ConvertToCodenameCodeFragments_GetCodenameCodeFragmentsFromOneFile()
        {
            var codeSampleFile = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                    {
                        new CodeFragment
                        {
                            Codename = "hello-world",
                            Content = "console.log('Hello Kentico Cloud');",
                            Language = CodeFragmentLanguage.JavaScript
                        },

                        new CodeFragment
                        {
                            Codename = "hello-world",
                            Content = "System.out.println(\"Hello, World\");",
                            Language = CodeFragmentLanguage.Java
                        }
                    }
            };
                
            var expectedOutput = new List<CodenameCodeFragments>
            {
                new CodenameCodeFragments
                {
                    Codename = "hello-world",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {CodeFragmentLanguage.JavaScript, "console.log('Hello Kentico Cloud');"},
                        {CodeFragmentLanguage.Java, "System.out.println(\"Hello, World\");"}
                    }
                }
            };

            var actualOutput = _codeSamplesConverter.ConvertToCodenameCodeFragments(codeSampleFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodenameCodeSamplesComparer());
        }

        [Test]
        public void ConvertToCodenameCodeFragments_GetDiferentCodenameCodeFragmentsFromOneFile()
        {
            var codeFile = new CodeFile
            {
                CodeFragments = new List<CodeFragment>
                    {
                        new CodeFragment
                        {
                            Codename = "hello-world",
                            Content = "console.log('Hello Kentico Cloud');",
                            Language = CodeFragmentLanguage.JavaScript
                        },

                        new CodeFragment
                        {
                            Codename = "hello-world-2",
                            Content = "System.out.println(\"Hello, World\");",
                            Language = CodeFragmentLanguage.Java
                        }
                    }
            };

            var expectedOutput = new List<CodenameCodeFragments>
            {
                new CodenameCodeFragments
                {
                    Codename = "hello-world",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {CodeFragmentLanguage.JavaScript, "console.log('Hello Kentico Cloud');"},
                    }
                },

                new CodenameCodeFragments
                {
                    Codename = "hello-world-2",
                    CodeFragments = new Dictionary<CodeFragmentLanguage, string>
                    {
                        {CodeFragmentLanguage.Java, "System.out.println(\"Hello, World\");"},
                    }
                }
            };

            var actualOutput = _codeSamplesConverter.ConvertToCodenameCodeFragments(codeFile);

            Assert.That(expectedOutput, Is.EqualTo(actualOutput).UsingCodenameCodeSamplesComparer());
        }
    }
}
