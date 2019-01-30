using System.Collections.Generic;
using GithubService.Models.Webhooks;
using GithubService.Services.Interfaces;
using GithubService.Services.Parsers;
using NUnit.Framework;

namespace GithubService.Services.Tests.Parsers
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
        public void ExtractFiles_WebhookMessageSingleCommit_ReturnsCorrectTupleWithAddedModifiedDeletedFiles()
        {
            var expectedAddedFiles = new[] {TestFileA};
            var expectedModifiedFiles = new string[] { };
            var expectedRemovedFiles = new[] {TestFileC, TestFileD};
            var expectedOutput = (expectedAddedFiles, expectedModifiedFiles, expectedRemovedFiles);
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

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ExtractFiles_WebhookMessageMultipleCommits_ReturnsCorrectTupleWithAddedModifiedDeletedFiles()
        {
            var expectedAddedFiles = new[] {TestFileB, TestFileC};
            var expectedModifiedFiles = new[] {TestFileD};
            var expectedRemovedFiles = new string[] { };
            var expectedOutput = (expectedAddedFiles, expectedModifiedFiles, expectedRemovedFiles);
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

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ExtractFiles_FileAddedAndThenModified_ReturnsAsAddedFile()
        {
            var expectedAddedFiles = new [] {TestFileA};
            var expectedOutput = (expectedAddedFiles, new string[] { }, new string[] { });
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>
                {
                    new Commit
                    {
                        Added = new List<string> {TestFileA},
                        Modified = new List<string>(),
                        Removed = new List<string>()
                    },
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string> {TestFileA},
                        Removed = new List<string>()
                    }
                }
            };

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ExtractFiles_FileAddedAndModifiedAndThenRemoved_ReturnsNoFile()
        {
            var expectedOutput = (new string[] { }, new string[] { }, new string[] { });
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>
                {
                    new Commit
                    {
                        Added = new List<string> {TestFileA},
                        Modified = new List<string>(),
                        Removed = new List<string>()
                    },
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string> {TestFileA},
                        Removed = new List<string>()
                    },
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string>(),
                        Removed = new List<string> {TestFileA}
                    }
                }
            };

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ExtractFiles_FileAddedAndThenRemoved_ReturnsNoFile()
        {
            var expectedOutput = (new string[] { }, new string[] { }, new string[] { });
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>
                {
                    new Commit
                    {
                        Added = new List<string> {TestFileA},
                        Modified = new List<string>(),
                        Removed = new List<string>()
                    },
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string>(),
                        Removed = new List<string> {TestFileA}
                    }
                }
            };

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ExtractFiles_FileRemovedAndThenAdded_ReturnsAsAddedFile()
        {
            var expectedAddedFiles = new [] {TestFileA};
            var expectedOutput = (expectedAddedFiles, new string[] { }, new string[] { });
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>
                {
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string>(),
                        Removed = new List<string> {TestFileA}
                    },
                    new Commit
                    {
                        Added = new List<string> {TestFileA},
                        Modified = new List<string>(),
                        Removed = new List<string>()
                    }
                }
            };

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ExtractFiles_FileModifiedAndThenRemoved_ReturnsAsRemovedFile()
        {
            var expectedRemovedFiles = new [] {TestFileA};
            var expectedOutput = (new string[] { }, new string[] { }, expectedRemovedFiles);
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>
                {
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string> {TestFileA},
                        Removed = new List<string>()
                    },
                    new Commit
                    {
                        Added = new List<string>(),
                        Modified = new List<string>(),
                        Removed = new List<string> {TestFileA}
                    }
                }
            };

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ExtractFiles_WebhookMessageNoCommit_ReturnsEmptyTuple()
        {
            var expectedOutput = (new string[] { }, new string[] { }, new string[] { });
            var webhookMessage = new WebhookMessage
            {
                Commits = new List<Commit>()
            };

            var actualOutput = _webhookParser.ExtractFiles(webhookMessage);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }
    }
}
