using GithubService.Models;

namespace GithubService.Services.Interfaces
{
    public interface IFileParser
    {
        CodeFile ParseContent(string filePath, string content);
    }
}
