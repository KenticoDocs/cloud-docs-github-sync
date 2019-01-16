using Microsoft.WindowsAzure.Storage.Table;

namespace GithubService.Repository
{
    public class CodeSampleFileEntity : TableEntity
    {
        public string Path { get; set; }

        public string CodeSamples { get; set; }

        public bool IsArchived { get; set; }
    }
}