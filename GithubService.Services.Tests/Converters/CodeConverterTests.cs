using GithubService.Models;
using GithubService.Services.Converters;
using GithubService.Services.Interfaces;
using GithubService.Services.Tests.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace GithubService.Services.Tests.Converters
{
    internal class CodeConverterTests
    {
        private readonly ICodeConverter _codeConverter = new CodeConverter();

        [Test]
        public void ConvertToCodenameCodeFragments_OnEmptyCodeFragments_ReturnsEmptyList()
        {
            var expectedOutput = new List<CodenameCodeFragments>();

            var actualOutput =
                _codeConverter.ConvertToCodenameCodeFragments(new List<CodeFragment>());

            Assert.That(actualOutput.ToList(), Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ConvertToCodenameCodeFragments_HandlesMultipleCodeFragmentsInOneFile()
        {
            var codeFragments = new List<CodeFragment>
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
            };

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

            var actualOutput = _codeConverter.ConvertToCodenameCodeFragments(codeFragments);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodenameCodeFragmentsComparer());
        }

        [Test]
        public void ConvertToCodenameCodeFragments_ComposesFragmentsFromMultipleFilesIntoOneObject()
        {
            var codeFragments = new List<CodeFragment>
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
                    Content = "Console.WriteLine(\"Hello World!\");",
                    Language = CodeFragmentLanguage.CSharp
                },
                new CodeFragment
                {
                    Codename = "hello-world",
                    Content = "print(\"Hello, World!\")",
                    Language = CodeFragmentLanguage.PHP
                },
                new CodeFragment
                {
                    Codename = "hello-world",
                    Content = "System.out.println(\"Hello, World\");",
                    Language = CodeFragmentLanguage.Java
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

            var actualOutput = _codeConverter.ConvertToCodenameCodeFragments(codeFragments);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput).UsingCodenameCodeFragmentsComparer());
        }

        [Test]
        public void CompareFragmentLists_DetectsNewCodeFragment()
        {
            var oldList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Codename = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var newList = new List<CodeFragment>
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
                    Content = "Console.WriteLine(\"Hello World!\");",
                    Language = CodeFragmentLanguage.CSharp
                }
            };

            var expectedFragment = new CodeFragment
            {
                Codename = "hello-world",
                Content = "Console.WriteLine(\"Hello World!\");",
                Language = CodeFragmentLanguage.CSharp
            };

            var (newFragments, modifiedFragments, removedFragments) = _codeConverter.CompareFragmentLists(oldList, newList);

            Assert.That(newFragments, Does.Contain(expectedFragment).UsingCodeFragmentComparer());
            Assert.That(modifiedFragments, Is.Empty);
            Assert.That(removedFragments, Is.Empty);
        }

        [Test]
        public void CompareFragmentLists_DetectsModifiedCodeFragment()
        {
            var oldList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Codename = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Codename = "hello-world",
                    Content = "console.log('Hello Awesome Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var expectedFragment = new CodeFragment
            {
                Codename = "hello-world",
                Content = "console.log('Hello Awesome Kentico Cloud');",
                Language = CodeFragmentLanguage.JavaScript
            };

            var (newFragments, modifiedFragments, removedFragments) = _codeConverter.CompareFragmentLists(oldList, newList);

            Assert.That(modifiedFragments, Does.Contain(expectedFragment).UsingCodeFragmentComparer());
            Assert.That(newFragments, Is.Empty);
            Assert.That(removedFragments, Is.Empty);
        }

        [Test]
        public void CompareFragmentLists_DetectsRemovedCodeFragment()
        {
            var oldList = new List<CodeFragment>
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
                    Content = "Console.WriteLine(\"Hello World!\");",
                    Language = CodeFragmentLanguage.CSharp
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Codename = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var expectedFragment = new CodeFragment
            {
                Codename = "hello-world",
                Content = "Console.WriteLine(\"Hello World!\");",
                Language = CodeFragmentLanguage.CSharp
            };

            var (newFragments, modifiedFragments, removedFragments) = _codeConverter.CompareFragmentLists(oldList, newList);

            Assert.That(removedFragments, Does.Contain(expectedFragment).UsingCodeFragmentComparer());
            Assert.That(newFragments, Is.Empty);
            Assert.That(modifiedFragments, Is.Empty);
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("test", "Test")]
        [TestCase("random_test", "Random Test")]
        [TestCase("cool_kentico_api", "Cool Kentico Api")]
        [TestCase("cool__api", "Cool  Api")]
        [TestCase("1_2_3", "1 2 3")]
        [TestCase("wRoNg_CASing", "Wrong Casing")]
        public void ConvertCodenameToItemName(string codename, string expectedName)
        {
            var actualName = _codeConverter.ConvertCodenameToItemName(codename);

            Assert.That(actualName, Is.EqualTo(expectedName));
        }
    }
}
