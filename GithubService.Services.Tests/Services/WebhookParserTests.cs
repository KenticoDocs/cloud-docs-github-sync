using System.Collections.Generic;
using GithubService.Models.Webhooks;
using GithubService.Services.Interfaces;
using GithubService.Services.Services;
using NUnit.Framework;

namespace GithubService.Services.Tests.Services
{
    [TestFixture]
    internal class WebhookParserTests
    {
        private const string TestFileA = "SomeDirectory/testFile.a";
        private const string TestFileB = "SomeDirectory/testFile.b";
        private const string TestFileC = "AnotherDirectory/MoreDirectory/testFile.c";
        private const string TestFileD = "testFile.d";
        private const string TestFileE = "testFile.e";

        private readonly IWebhookParser _webhookParser = new WebhookParser();

        [Test]
        public void ParseFiles_WebhookMessageSingleCommit_ReturnsTupleWithAddedModifiedDeletedFiles()
        {
            var expectedAddedFiles = new[] {TestFileA};
            var expectedModifiedFiles = new string[] { };
            var expectedRemovedFiles = new[] {TestFileC, TestFileD};
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>
                {
                    new Commit
                    {
                        Added = new List<string>(expectedAddedFiles),
                        Modified = new List<string>(expectedModifiedFiles),
                        Removed = new List<string>(expectedRemovedFiles)
                    }
                }
            };

            var (addedFiles, modifiedFiles, removedFiles) = _webhookParser.ParseFiles(webhookMessage);

            CollectionAssert.AreEquivalent(addedFiles, expectedAddedFiles);
            CollectionAssert.AreEquivalent(modifiedFiles, expectedModifiedFiles);
            CollectionAssert.AreEquivalent(removedFiles, expectedRemovedFiles);
        }

        [Test]
        public void ParseFiles_WebhookMessageMultipleCommits_ReturnsTupleWithAddedModifiedDeletedFiles()
        {
            var expectedAddedFiles = new[] {TestFileB, TestFileC};
            var expectedModifiedFiles = new[] {TestFileD};
            var expectedRemovedFiles = new string[] { };
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>
                {
                    new Commit
                    {
                        Added = new List<string> {TestFileA, TestFileB, TestFileE},
                        Modified = new List<string> {TestFileD},
                        Removed = new List<string> {TestFileC}
                    },
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string> {TestFileB},
                        Removed = new List<string>()
                    },
                    new Commit
                    {
                        Added = new List<string> {TestFileC},
                        Modified = new List<string> {TestFileB, TestFileD, TestFileE},
                        Removed = new List<string> {TestFileA}
                    },
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string>(),
                        Removed = new List<string> {TestFileE}
                    }
                }
            };

            var (addedFiles, modifiedFiles, removedFiles) = _webhookParser.ParseFiles(webhookMessage);

            CollectionAssert.AreEquivalent(addedFiles, expectedAddedFiles);
            CollectionAssert.AreEquivalent(modifiedFiles, expectedModifiedFiles);
            CollectionAssert.AreEquivalent(removedFiles, expectedRemovedFiles);
        }
    }
}