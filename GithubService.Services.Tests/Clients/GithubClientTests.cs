using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GithubService.Models.Github;
using GithubService.Services.Clients;
using GithubService.Services.Interfaces;
using GithubService.Services.Tests.Utils;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace GithubService.Services.Tests.Clients
{
    internal class GithubClientTests
    {
        private const string RepositoryName = "my-repo";
        private const string RepositoryOwner = "me-owner";
        private const string RepositoryAccessToken = "secret-access-token";
        private readonly string _baseAddress = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}";
        private readonly IGithubClient _githubClient;
        private readonly MockHttpMessageHandler _mockHttpClient;

        public GithubClientTests()
        {
            _mockHttpClient = new MockHttpMessageHandler();
            _githubClient = new GithubClient(
                _mockHttpClient.ToHttpClient(),
                RepositoryName,
                RepositoryOwner,
                RepositoryAccessToken
            );
        }

        [SetUp]
        public void SetUp()
        {
            _mockHttpClient.Clear();
        }

        [Test]
        public async Task GetFileContentAsync_FilePath_ReturnsCorrectContent()
        {
            const string expectedContent = "Console.log('Hello world');";
            const string filePath = "some/file/path";
            var encodedPath = Uri.EscapeDataString(filePath);
            var encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedContent));
            var fileContent = "{ \"content\": \" " + encodedContent + " \" }";
            _mockHttpClient
                .When($"{_baseAddress}/contents/{encodedPath}")
                .Respond("application/json", fileContent);

            var extractedContent = await _githubClient.GetFileContentAsync(filePath);

            Assert.AreEqual(expectedContent, extractedContent);
        }

        [Test]
        public void GetFileContentAsync_FilePath_ThrowsUnsuccessfulRequestException()
        {
            const string filePath = "some/file/path";
            var encodedPath = Uri.EscapeDataString(filePath);
            _mockHttpClient
                .When($"{_baseAddress}/contents/{encodedPath}")
                .Respond(HttpStatusCode.BadRequest, message => new StringContent("This is a bad request"));

            Assert.ThrowsAsync<UnsuccessfulRequestException>(async () =>
                await _githubClient.GetFileContentAsync(filePath));
        }

        [Test]
        public async Task GetBlobContentAsync_BlobId_ReturnsBlobContent()
        {
            const string blobId = "a09d1f88-9032-4954-9a5b-d467d924f4ee";
            const string expectedContent = "my awesome blob content";
            var encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedContent));
            var blobContent = "{ \"content\": \" " + encodedContent + " \" }";
            _mockHttpClient
                .When($"{_baseAddress}/git/blobs/{blobId}")
                .Respond("application/json", blobContent);

            var extractedContent = await _githubClient.GetBlobContentAsync(blobId);

            Assert.AreEqual(expectedContent, extractedContent);
        }

        [Test]
        public async Task GetTreeNodesRecursivelyAsync_ReturnsGithubTreeNode()
        {
            const string treeSha = "b4eecafa9be2f2006ce1b709d6857b07069b4608";
            var branchContent = $@"{{
""commit"": {{
  ""commit"": {{
      ""tree"": {{
        ""sha"": ""{treeSha}"",
      }},
    }},
  }},
}}";
            const string node1Path = "file.rb";
            const string node1Id = "44b4fc6d56897b048c772eb4087f854f46256132";
            const string node1Type = "blob";
            const string node2Path = "subdir";
            const string node2Id = "f484d249c660418515fb01c2b9662073663c242e";
            const string node2Type = "tree";
            var treeContent = $@"{{
  ""tree"": [
    {{
      ""path"": ""{node1Path}"",
      ""mode"": ""100644"",
      ""type"": ""{node1Type}"",
      ""size"": 30,
      ""sha"": ""{node1Id}"",
      ""url"": ""https://api.github.com/repos/octocat/Hello-World/git/blobs/44b4fc6d56897b048c772eb4087f854f46256132""
    }},
    {{
      ""path"": ""{node2Path}"",
      ""mode"": ""040000"",
      ""type"": ""{node2Type}"",
      ""sha"": ""{node2Id}"",
      ""url"": ""https://api.github.com/repos/octocat/Hello-World/git/blobs/f484d249c660418515fb01c2b9662073663c242e""
    }}
  ],
  ""truncated"": false
}}";
            _mockHttpClient
                .When($"{_baseAddress}/branches/master")
                .Respond("application/json", branchContent);
            _mockHttpClient
                .When($"{_baseAddress}/git/trees/{treeSha}?recursive=1")
                .Respond("application/json", treeContent);
            var expectedNodes = new List<GithubTreeNode>
            {
                new GithubTreeNode
                {
                    Id = node1Id,
                    Path = node1Path,
                    Type = node1Type
                },
                new GithubTreeNode
                {
                    Id = node2Id,
                    Path = node2Path,
                    Type = node2Type
                }
            };

            var nodes = await _githubClient.GetTreeNodesRecursivelyAsync();

            Assert.That(nodes, Is.EquivalentTo(expectedNodes).UsingGithubNodeTreeComparer());
        }
    }
}