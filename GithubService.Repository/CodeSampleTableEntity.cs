using Microsoft.WindowsAzure.Storage.Table;

namespace GithubService.Repository
{
    public class CodeSampleFileDto : TableEntity
    {
        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set
            {
                PartitionKey = value;
                RowKey = value;
                _filePath = value;
            }
        }

        public string CodeSamples { get; set; }
    }
}
