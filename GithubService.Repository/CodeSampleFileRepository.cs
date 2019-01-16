using GithubService.Models.CodeSamples;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GithubService.Repository
{
    public class CodeSampleFileRepository : ICodeSampleFileRepository
    {
        private static readonly string TableName = "CodeSampleFile";
        private static readonly string EntityRowKey = "csf";

        private readonly CloudTable _fileCodeSamplesTable;

        public static async Task<CodeSampleFileRepository> CreateInstance(string connectionString)
        {
            var instance = new CodeSampleFileRepository(connectionString);
            await instance._fileCodeSamplesTable.CreateIfNotExistsAsync();
            return instance;
        }

        private CodeSampleFileRepository(string connectionString)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = cloudStorageAccount.CreateCloudTableClient();

            _fileCodeSamplesTable = tableClient.GetTableReference(TableName);
        }

        public async Task<CodeSampleFile> GetAsync(string filePath)
        {
            var entity = await GetEntityAsync(filePath);

            if (entity == null || entity.IsArchived)
            {
                return null;
            }

            return new CodeSampleFile
            {
                FilePath = filePath,
                CodeSamples = JsonConvert.DeserializeObject<List<CodeSample>>(entity.CodeSamples)
            };
        }

        public async Task StoreAsync(CodeSampleFile file)
        {
            var entity = new CodeSampleFileEntity
            {
                PartitionKey = ConstructPartitionKey(file.FilePath),
                RowKey = EntityRowKey,
                Path = file.FilePath,
                CodeSamples = JsonConvert.SerializeObject(file.CodeSamples)
            };

            await StoreEntityAsync(entity);
        }

        public async Task ArchiveAsync(CodeSampleFile file)
        {
            var entity = await GetEntityAsync(file.FilePath);

            if (entity != null)
            {
                entity.IsArchived = true;
                await StoreEntityAsync(entity);
            }
        }

        private async Task<CodeSampleFileEntity> GetEntityAsync(string filePath)
        {
            var operation = TableOperation.Retrieve<CodeSampleFileEntity>(ConstructPartitionKey(filePath), EntityRowKey);

            var retrievedResult = await _fileCodeSamplesTable.ExecuteAsync(operation);

            if (retrievedResult.Result == null)
            {
                return null;
            }

            return (CodeSampleFileEntity)retrievedResult.Result;
        }

        private async Task StoreEntityAsync(CodeSampleFileEntity entity)
        {
            var operation = TableOperation.InsertOrReplace(entity);

            await _fileCodeSamplesTable.ExecuteAsync(operation);
        }

        private string ConstructPartitionKey(string filePath)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(filePath));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}
