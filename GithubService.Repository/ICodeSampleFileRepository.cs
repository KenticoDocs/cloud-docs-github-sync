using GithubService.Models.CodeSamples;

namespace GithubService.Repository
{
    public interface ICodeSampleFileRepository
    {
        CodeSampleFile GetFile(string filePath);

        CodeSampleFile UpdateFile(string filePath, CodeSampleFile updatedFile);

        CodeSampleFile ArchiveFile(string filePath);
    }
}
