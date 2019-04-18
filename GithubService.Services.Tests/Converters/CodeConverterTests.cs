using GithubService.Models;
using GithubService.Services.Converters;
using GithubService.Services.Interfaces;
using GithubService.Services.Tests.Utils;
using NUnit.Framework;
using System.Collections.Generic;

namespace GithubService.Services.Tests.Converters
{
    internal class CodeConverterTests
    {
        private readonly ICodeConverter _codeConverter = new CodeConverter();

        [Test]
        public void CompareFragmentLists_DetectsNewCodeFragment()
        {
            var oldList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript
                },
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "Console.WriteLine(\"Hello World!\");",
                    Language = CodeFragmentLanguage.CSharp,
                    Platform = CodeFragmentPlatform.Net
                }
            };

            var expectedFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "Console.WriteLine(\"Hello World!\");",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net
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
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "console.log('Hello Awesome Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript
                }
            };

            var expectedFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "console.log('Hello Awesome Kentico Cloud');",
                Language = CodeFragmentLanguage.JavaScript,
                Platform = CodeFragmentPlatform.JavaScript
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
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript
                },
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "Console.WriteLine(\"Hello World!\");",
                    Language = CodeFragmentLanguage.CSharp,
                    Platform = CodeFragmentPlatform.Net
                }
            };

            var newList = new List<CodeFragment>
            {
                new CodeFragment
                {
                    Identifier = "hello-world",
                    Content = "console.log('Hello Kentico Cloud');",
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript
                }
            };

            var expectedFragment = new CodeFragment
            {
                Identifier = "hello-world",
                Content = "Console.WriteLine(\"Hello World!\");",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net
            };

            var (newFragments, modifiedFragments, removedFragments) = _codeConverter.CompareFragmentLists(oldList, newList);

            Assert.That(removedFragments, Does.Contain(expectedFragment).UsingCodeFragmentComparer());
            Assert.That(newFragments, Is.Empty);
            Assert.That(modifiedFragments, Is.Empty);
        }
    }
}
