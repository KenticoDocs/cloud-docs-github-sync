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
            var files = new List<CodeSampleFile>();
            var githubTree = await _githubClient.GetTreeFilesRecursively();
            
            foreach (var blob in githubTree.Where(file => file.Type == "blob"))
            {
                var content = await _githubClient.GetBlobContentAsync(blob.Id);
                var file = _fileParser.ParseContent(blob.Path, content);

                files.Add(file);
            }

            return files;
        }

        public CodeSampleFile GetCodeSampleFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
