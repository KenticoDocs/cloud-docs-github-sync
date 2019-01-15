using GithubService.Models.CodeSamples;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubService.Repository
{
    public class CodeSampleFileRepository : ICodeSampleFileRepository
    {
        private readonly CloudTable _fileCodeSamplesTable;

        public static async Task<CodeSampleFileRepository> CreateInstance(ICodeSampleFileRepositoryConfig config)
        {
            var instance = new CodeSampleFileRepository(config);
            await instance._fileCodeSamplesTable.CreateIfNotExistsAsync();
            return instance;
        }

        private CodeSampleFileRepository(ICodeSampleFileRepositoryConfig config)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(config.ConnectionString);
            var tableClient = cloudStorageAccount.CreateCloudTableClient();

            _fileCodeSamplesTable = tableClient.GetTableReference(config.TableName);
        }

        private CodeSampleFileRepository() { }

        public Task ArchiveFileAsync(CodeSampleFile file)
        {
            throw new NotImplementedException();
        }

        public async Task<CodeSampleFile> GetFileAsync(string filePath)
        {
            var partitionKey = filePath.ToPartitionKey();
            var query = new TableQuery<CodeSampleTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var codeSampleEntities = new List<CodeSampleTableEntity>();
            TableContinuationToken token = null;

            do
            {
                var seg = await _fileCodeSamplesTable.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                codeSampleEntities.AddRange(seg);

            } while (token != null);

            var codeSamples = codeSampleEntities
                .Select(sample => sample.ToCodeSample())
                .ToList();

            return new CodeSampleFile
            {
                FilePath = filePath,
                CodeSamples = codeSamples,
            };
        }

        public Task<CodeSampleFile> UpdateFileAsync(CodeSampleFile fileToUpdate)
        {
            throw new NotImplementedException();
        }

        public async Task AddFileAsync(CodeSampleFile newFile)
        {
            var batchOperation = new TableBatchOperation();
            var partitionKey = newFile.FilePath.ToPartitionKey();

            foreach (var sample in newFile.CodeSamples)
                batchOperation.Insert(sample.ToTableEntity(partitionKey));

            await _fileCodeSamplesTable.ExecuteBatchAsync(batchOperation);
        }
    }
}
