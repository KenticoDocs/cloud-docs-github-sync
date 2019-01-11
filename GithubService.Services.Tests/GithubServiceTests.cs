using System.Collections.Generic;
using GithubService.Models.CodeSamples;
using GithubService.Models.Github;
using GithubService.Services.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace GithubService.Services.Tests
{
    public class GithubServiceTests
    {
        private static readonly GithubTreeFile FileA =
            new GithubTreeFile { Path = "PathA", Id = "ShaA", Type = "blob" };
        private static readonly GithubTreeFile FileB =
            new GithubTreeFile { Path = "PathB", Id = "ShaB", Type = "blob" };
        private static readonly GithubTreeFile FileC =
            new GithubTreeFile { Path = "PathC", Id = "ShaC", Type = "tree" };
        private static readonly GithubTreeFile FileD =
            new GithubTreeFile { Path = "PathD", Id = "ShaD", Type = "blob" };
        private static readonly GithubTreeFile FileE =
            new GithubTreeFile { Path = "PathE", Id = "ShaE", Type = "tree" };
        
        [Test]
        public void GetCodeSamplesFiles_ReturnsCorrectFiles()
        {
            // Arrange
            var githubFiles = new List<GithubTreeFile> { FileA, FileB, FileC, FileD, FileE };

            var fileWithSamplesA = new CodeSampleFile { FilePath = FileA.Path };
            var fileWithSamplesB = new CodeSampleFile { FilePath = FileB.Path };
            var fileWithSamplesD = new CodeSampleFile { FilePath = FileD.Path };
            var expectedResult = new List<CodeSampleFile> { fileWithSamplesA, fileWithSamplesB, fileWithSamplesD };

            var blobAContent = "Path_A_Content";
            var blobBContent = "Path_B_Content";
            var blobDContent = "Path_D_Content";

            var githubClient = Substitute.For<IGithubClient>();
            githubClient.GetTreeFilesRecursively().Returns(githubFiles);
            githubClient.GetBlobContentAsync(FileA.Id).Returns(blobAContent);
            githubClient.GetBlobContentAsync(FileB.Id).Returns(blobBContent);
            githubClient.GetBlobContentAsync(FileD.Id).Returns(blobDContent);

            var fileParser = Substitute.For<IFileParser>();
            fileParser.ParseContent(FileA.Path, blobAContent).Returns(fileWithSamplesA);
            fileParser.ParseContent(FileB.Path, blobBContent).Returns(fileWithSamplesB);
            fileParser.ParseContent(FileD.Path, blobDContent).Returns(fileWithSamplesD);

            // Act
            var githubService = new GithubService(githubClient, fileParser);
            var result = githubService.GetCodeSampleFilesAsync().Result;

            // Assert
            CollectionAssert.AreEquivalent(expectedResult, result);
        }
        
        [Test]
        public void GetCodeSamplesFiles_OnlyTrees_ReturnsEmptyList()
        {
            // Arrange
            var githubFiles = new List<GithubTreeFile> { FileC, FileE };
            var expectedResult = new List<CodeSampleFile>();

            var githubClient = Substitute.For<IGithubClient>();
            githubClient.GetTreeFilesRecursively().Returns(githubFiles);

            var fileParser = Substitute.For<IFileParser>();

            // Act
            var githubService = new GithubService(githubClient, fileParser);
            var result = githubService.GetCodeSampleFilesAsync().Result;

            // Assert
            CollectionAssert.AreEquivalent(expectedResult, result);
        }
    }
}