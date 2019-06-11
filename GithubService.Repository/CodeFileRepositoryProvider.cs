using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;

namespace GithubService.Repository
{
    public static class CodeFileRepositoryProvider
    {
        private const string TableName = "CodeFile";

        public static async Task<CodeFileRepository> CreateCodeFileRepositoryInstance(string connectionString)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = cloudStorageAccount.CreateCloudTableClient();
            var codeFileTable = tableClient.GetTableReference(TableName);

            await codeFileTable.CreateIfNotExistsAsync();

            return new CodeFileRepository(codeFileTable);
        }
    }
}
