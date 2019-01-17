using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubService.Models.CodeSamples;
using GithubService.Services.Interfaces;

namespace GithubService.Services
{
    public class GithubService : IGithubService
    {
        private readonly IGithubClient _githubClient;
        private readonly IFileParser _fileParser;

        public GithubService(IGithubClient githubClient, IFileParser fileParser)
        {
            _githubClient = githubClient;
            _fileParser = fileParser;
        }

        public async Task<IEnumerable<CodeSampleFile>> GetCodeSampleFilesAsync()
        {
            var nodes = await _githubClient.GetTreeNodesRecursivelyAsync();
            var files = nodes.Where(node => node.Type == "blob");

            return await Task.WhenAll(files.Select(async file => {
                var content = await _githubClient.GetBlobContentAsync(file.Id);
                return _fileParser.ParseContent(file.Path, content);
            }));
        }

        public Task<CodeSampleFile> GetCodeSampleFileAsync(string path)
        {
            throw new NotImplementedException();
        }
    }
}
