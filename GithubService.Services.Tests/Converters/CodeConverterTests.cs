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
        public void ConvertToCodeSamples_OnEmptyCodeFragments_ReturnsEmptyList()
        {
            var expectedOutput = new List<CodenameCodeFragments>();

            var actualOutput =
                _codeConverter.ConvertToCodenameCodeFragments(new List<CodeFragment>());

            Assert.That(actualOutput.ToList(), Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ConvertToCodeSamples_HandlesOneCodeSampleInMultipleLanguages()
        {
            var javascriptFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "console.log('Hello Kentico Cloud');",
                Language = CodeFragmentLanguage.JavaScript
            };

            var netFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "System.out.println('Hello Kentico Cloud');",
                Language = CodeFragmentLanguage.Net
            };

            var javaFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "int i += 3",
                Language = CodeFragmentLanguage.Java
            };

            var codeFragments = new List<CodeFragment>
            {
                javaFragment,
                javascriptFragment,
                netFragment
            };

            var expectedOutput = new List<CodenameCodeFragments>
            {
                new CodenameCodeFragments
                {
                    Codename = "hello-world",
                    CodeFragments = new List<CodeFragment>
                    {
                        javaFragment,
                        javascriptFragment,
                        netFragment
                    }
                },
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
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                },
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "Console.WriteLine(\"Hello World!\");",
                    Language = CodeFragmentLanguage.Net
                }
            };

            var expectedFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "Console.WriteLine(\"Hello World!\");",
                Language = CodeFragmentLanguage.Net
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
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "console.log('Hello Awesome Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var expectedFragment = new CodeFragment
            {
                Identifier = "hello-world",
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
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                },
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "Console.WriteLine(\"Hello World!\");",
                    Language = CodeFragmentLanguage.Net
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript
                }
            };

            var expectedFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "Console.WriteLine(\"Hello World!\");",
                Language = CodeFragmentLanguage.Net
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
