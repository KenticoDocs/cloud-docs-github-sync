using GithubService.Models.Github;
using GithubService.Services.Interfaces;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Models;

namespace GithubService.Services.Tests
{
    public class GithubServiceTests
    {
        private static readonly GithubTreeNode NodeA =
            new GithubTreeNode { Path = "PathA", Id = "ShaA", Type = "blob" };
        private static readonly GithubTreeNode NodeB =
            new GithubTreeNode { Path = "PathB", Id = "ShaB", Type = "blob" };
        private static readonly GithubTreeNode NodeC =
            new GithubTreeNode { Path = "PathC", Id = "ShaC", Type = "tree" };
        private static readonly GithubTreeNode NodeD =
            new GithubTreeNode { Path = "PathD", Id = "ShaD", Type = "blob" };
        private static readonly GithubTreeNode NodeE =
            new GithubTreeNode { Path = "PathE", Id = "ShaE", Type = "tree" };

        [Test]
        public async Task GetCodeSampleFiles_ReturnsCorrectFiles()
        {
            // Arrange
            var treeNodes = new List<GithubTreeNode> { NodeA, NodeB, NodeC, NodeD, NodeE };

            var codeSampleFileA = new CodeFile { FilePath = NodeA.Path };
            var codeSampleFileB = new CodeFile { FilePath = NodeB.Path };
            var codeSampleFileD = new CodeFile { FilePath = NodeD.Path };
            var expectedResult = new List<CodeFile> { codeSampleFileA, codeSampleFileB, codeSampleFileD };

            var blobAContent = "Path_A_Content";
            var blobBContent = "Path_B_Content";
            var blobDContent = "Path_D_Content";

            var githubClient = Substitute.For<IGithubClient>();
            githubClient.GetTreeNodesRecursivelyAsync().Returns(treeNodes);
            githubClient.GetBlobContentAsync(NodeA.Id).Returns(blobAContent);
            githubClient.GetBlobContentAsync(NodeB.Id).Returns(blobBContent);
            githubClient.GetBlobContentAsync(NodeD.Id).Returns(blobDContent);

            var fileParser = Substitute.For<IFileParser>();
            fileParser.ParseContent(NodeA.Path, blobAContent).Returns(codeSampleFileA);
            fileParser.ParseContent(NodeB.Path, blobBContent).Returns(codeSampleFileB);
            fileParser.ParseContent(NodeD.Path, blobDContent).Returns(codeSampleFileD);

            // Act
            var githubService = new GithubService(githubClient, fileParser);
            var result = await githubService.GetCodeFilesAsync();

            // Assert
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        [Test]
        public async Task GetCodeSampleFiles_OnlyTrees_ReturnsEmptyList()
        {
            // Arrange
            var treeNodes = new List<GithubTreeNode> { NodeC, NodeE };
            var expectedResult = new List<CodeFile>();

            var githubClient = Substitute.For<IGithubClient>();
            githubClient.GetTreeNodesRecursivelyAsync().Returns(treeNodes);

            var fileParser = Substitute.For<IFileParser>();

            // Act
            var githubService = new GithubService(githubClient, fileParser);
            var result = await githubService.GetCodeFilesAsync();

            // Assert
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        [Test]
        public async Task GetCodeSampleFileAsync_ReturnsCorrectFile()
        {
            // Arrange
            var filePath = "File_Path";
            var fileContent = "var three = 3;";
            var expectedCodeSample = new CodeSampleFile { FilePath = filePath };

            var githubClient = Substitute.For<IGithubClient>();
            githubClient.GetFileContentAsync(filePath).Returns(fileContent);

            var fileParser = Substitute.For<IFileParser>();
            fileParser.ParseContent(filePath, fileContent).Returns(expectedCodeSample);

            // Act
            var githubService = new GithubService(githubClient, fileParser);
            var result = await githubService.GetCodeSampleFileAsync(filePath);

            // Assert
            Assert.AreEqual(expectedCodeSample, result);
        }
    }
}