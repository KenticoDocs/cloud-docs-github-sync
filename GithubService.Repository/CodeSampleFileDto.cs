using Microsoft.WindowsAzure.Storage.Table;

namespace GithubService.Repository
{
    public class CodeSampleFileDto : TableEntity
    {
        public string FilePath
        {
            get => filePath;
            set { PartitionKey = value; RowKey = value; filePath = value; }
        }

        public string CodeSamples { get; set; }

        private string filePath;
    }
}
