using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface IFileParser
    {
        CodeSampleFile ParseContent(string filePath, string content);
    }
}
