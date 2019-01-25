using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GithubService.Models;
using GithubService.Repository.Models;

namespace GithubService.Repository
{
    public class CodeFileRepository : ICodeFileRepository
    {
        private static readonly string TableName = "CodeSampleFile";
        private static readonly string EntityRowKey = "csf";

        private readonly CloudTable _codeFilesTable;

        public static async Task<CodeFileRepository> CreateInstance(string connectionString)
        {
            var instance = new CodeFileRepository(connectionString);
            await instance._codeFilesTable.CreateIfNotExistsAsync();
            return instance;
        }

        private CodeFileRepository(string connectionString)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = cloudStorageAccount.CreateCloudTableClient();

            _codeFilesTable = tableClient.GetTableReference(TableName);
        }

        public async Task<CodeFile> GetAsync(string filePath)
        {
            var entity = await GetEntityAsync(filePath);

            if (entity == null || entity.IsArchived)
            {
                return null;
            }

            return new CodeFile
            {
                FilePath = filePath,
                CodeFragments = JsonConvert.DeserializeObject<List<CodeFragment>>(entity.CodeFragments)
            };
        }

        public async Task<CodeFile> StoreAsync(CodeFile file)
        {
            var entity = new CodeFileEntity
            {
                PartitionKey = ConstructPartitionKey(file.FilePath),
                RowKey = EntityRowKey,
                Path = file.FilePath,
                CodeFragments = JsonConvert.SerializeObject(file.CodeFragments)
            };

            var storedEntity = await StoreEntityAsync(entity);
            return new CodeFile
            {
                FilePath = storedEntity.Path,
                CodeFragments = JsonConvert.DeserializeObject<List<CodeFragment>>(storedEntity.CodeFragments)
            };
        }

        public async Task<CodeFile> ArchiveAsync(string filePath)
        {
            var entity = await GetEntityAsync(filePath);

            if (entity == null || entity.IsArchived)
                return null;

            entity.IsArchived = true;
            await StoreEntityAsync(entity);

            return new CodeFile
            {
                FilePath = entity.Path,
                CodeFragments = JsonConvert.DeserializeObject<List<CodeFragment>>(entity.CodeFragments)
            };
        }

        private async Task<CodeFileEntity> GetEntityAsync(string filePath)
        {
            var operation = TableOperation.Retrieve<CodeFileEntity>(ConstructPartitionKey(filePath), EntityRowKey);

            var retrievedResult = await _codeFilesTable.ExecuteAsync(operation);
            return (CodeFileEntity) retrievedResult.Result;
        }

        private async Task<CodeFileEntity> StoreEntityAsync(CodeFileEntity entity)
        {
            var operation = TableOperation.InsertOrReplace(entity);

            var result = await _codeFilesTable.ExecuteAsync(operation);
            return (CodeFileEntity) result.Result;
        }

        private string ConstructPartitionKey(string filePath)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(filePath));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}
