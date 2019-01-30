using Microsoft.WindowsAzure.Storage.Table;

namespace GithubService.Repository.Models
{
    public class CodeFileDto : TableEntity
    {
        public string FilePath
        {
            get => filePath;
            set { PartitionKey = value; RowKey = value; filePath = value; }
        }

        public string CodeFragments { get; set; }

        private string filePath;
    }
}
