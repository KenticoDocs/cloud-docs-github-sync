using GithubService.Models;
using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface IFileParser
    {
        FileCodeSamples ParseContent(string filePath, string content);
    }
}
