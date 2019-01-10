using GithubService.Models;
using GithubService.Models.CodeSamples;

namespace GithubService.Repository
{
    public interface IRepository
    {
        FileCodeSamples GetFile(string filePath);

        FileCodeSamples UpdateFile(string filePath, FileCodeSamples updatedFile);

        FileCodeSamples ArchiveFile(string filePath);
    }
}
