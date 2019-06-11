using GithubService.Models;
using GithubService.Repository.Models;
using Microsoft.WindowsAzure.Storage.Table;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Tests.Common.Comparers;
using CodeFragment = GithubService.Models.CodeFragment;

namespace GithubService.Repository.Tests
{
    public class CodeFileRepositoryTests
    {
        private CloudTable _codeFileTable;
        private const string TestFilePath = "test";

        [SetUp]
        public void Init() =>
            _codeFileTable = Substitute
                .For<CloudTable>(new Uri("http://127.0.0.1:10002/devstoreaccount1/codeFileRepository"));

        [Test]
        public async Task GetAsync_CodeFileEntityDoesNotExist_ReturnsNull()
        {
            // Arrange
            _codeFileTable
                .ExecuteAsync(Arg.Any<TableOperation>())
                .Returns(new TableResult { Result = null });

            var codeFileRepository = new CodeFileRepository(_codeFileTable);
            
            // Act
            var actualResult = await codeFileRepository.GetAsync(TestFilePath);

            // Assert
            Assert.AreEqual(null, actualResult);
        }

        [Test]
        public async Task GetAsync_CodeFileEntityIsArchived_ReturnsNull()
        {
            // Arrange
            _codeFileTable
                .ExecuteAsync(Arg.Any<TableOperation>())
                .Returns(new TableResult { Result = new CodeFileEntity { IsArchived = true } });

            var codeFileRepository = new CodeFileRepository(_codeFileTable);

            // Act
            var actualResult = await codeFileRepository.GetAsync(TestFilePath);

            // Assert
            Assert.AreEqual(null, actualResult);
        }

        [Test]
        public async Task GetAsync_CodeFileEntityIsNotArchived_ReturnsCorrectCodeFile()
        {
            // Arrange
            _codeFileTable
                .ExecuteAsync(Arg.Any<TableOperation>())
                .Returns(new TableResult { Result = new CodeFileEntity {
                    IsArchived = false,
                    CodeFragments = @"[{
                          ""Identifier"":""test_identifier"",
                          ""Content"":""test content"",
                          ""Language"":""c_"",
                          ""Platform"":""_net"",
                          ""Codename"":""test_identifier_net""
                        }]"
                    }
                });

            var codeFileRepository = new CodeFileRepository(_codeFileTable);

            var expectedCodeFragment = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = "c_",
                Platform = "_net",
            };
             
            var expectedResult = new CodeFile {
                FilePath = TestFilePath,
                CodeFragments = new List<CodeFragment> { expectedCodeFragment }
            };

            // Act
            var actualResult = await codeFileRepository.GetAsync(TestFilePath);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult).UsingCodeFileComparer());
        }

        [Test]
        public async Task StoreAsync_StoreCodeFile_ReturnsCorrectCodeFile()
        {
            // Arrange
            var codeFragmentsJson = @"[{
                ""Identifier"":""test_identifier"",
                ""Content"":""test content"",
                ""Language"":""c_"",
                ""Platform"":""_net"",
                ""Codename"":""test_identifier_net""
            }]";

            var expectedCodeFragment = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = "c_",
                Platform = "_net",
            };

            var expectedResult = new CodeFile
            {
                FilePath = TestFilePath,
                CodeFragments = new List<CodeFragment> { expectedCodeFragment }
            };

            _codeFileTable
                .ExecuteAsync(Arg.Any<TableOperation>())
                .Returns(new TableResult
                {
                    Result = new CodeFileEntity
                    {
                        IsArchived = false,
                        Path = TestFilePath,
                        CodeFragments = codeFragmentsJson,
                    }
                });

            var codeFileRepository = new CodeFileRepository(_codeFileTable);

            // Act
            var actualResult = await codeFileRepository.StoreAsync(expectedResult);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult).UsingCodeFileComparer());
        }

        [Test]
        public async Task ArchiveAsync_CodeFileEntityDoesNotExist_ReturnsNull()
        {
            // Arrange
            _codeFileTable
                .ExecuteAsync(Arg.Any<TableOperation>())
                .Returns(new TableResult { Result = null });

            var codeFileRepository = new CodeFileRepository(_codeFileTable);

            // Act
            var actualResult = await codeFileRepository.ArchiveAsync(TestFilePath);

            // Assert
            Assert.AreEqual(null, actualResult);
        }

        [Test]
        public async Task ArchiveAsync_CodeFileEntityIsArchived_ReturnsNull()
        {
            // Arrange
            _codeFileTable
                .ExecuteAsync(Arg.Any<TableOperation>())
                .Returns(new TableResult { Result = new CodeFileEntity { IsArchived = true } });

            var codeFileRepository = new CodeFileRepository(_codeFileTable);

            // Act
            var actualResult = await codeFileRepository.ArchiveAsync(TestFilePath);

            // Assert
            Assert.AreEqual(null, actualResult);
        }

        [Test]
        public async Task ArchiveAsync_ArchiveCodeFileEntity_ReturnsCorrectCodeFile()
        {
            // Arrange
            var codeFragmentsJson = @"[{
                ""Identifier"":""test_identifier"",
                ""Content"":""test content"",
                ""Language"":""c_"",
                ""Platform"":""_net"",
                ""Codename"":""test_identifier_net""
            }]";

            var expectedCodeFragment = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = "c_",
                Platform = "_net",
            };

            var expectedResult = new CodeFile
            {
                FilePath = TestFilePath,
                CodeFragments = new List<CodeFragment> { expectedCodeFragment }
            };

            var archivedCodeFileEntity = new CodeFileEntity { IsArchived = true, CodeFragments = codeFragmentsJson };

            _codeFileTable
                .ExecuteAsync(TableOperation.InsertOrReplace(archivedCodeFileEntity))
                .Returns(new TableResult
                {
                    Result = new CodeFileEntity
                    {
                        IsArchived = true,
                        CodeFragments = codeFragmentsJson,
                    }
                });

            _codeFileTable
                .ExecuteAsync(Arg.Any<TableOperation>())
                .Returns(new TableResult
                {
                    Result = new CodeFileEntity
                    {
                        IsArchived = false,
                        Path = TestFilePath,
                        CodeFragments = codeFragmentsJson,
                    }
                });

            var codeFileRepository = new CodeFileRepository(_codeFileTable);

            // Act
            var actualResult = await codeFileRepository.ArchiveAsync(TestFilePath);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult).UsingCodeFileComparer());
        }
    }
}
