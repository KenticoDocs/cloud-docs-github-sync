using Microsoft.WindowsAzure.Storage.Table;

namespace GithubService.Repository.Models
{
    public class CodeFileEntity : TableEntity
    {
        public string Path { get; set; }

        public string CodeFragments { get; set; }

        public bool IsArchived { get; set; }
    }
}