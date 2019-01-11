using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Models.CodeSamples;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace GithubService.Repository
{
    public class CodeSampleFileRepository : ICodeSampleFileRepository
    {
        private static readonly string tableName = "FileCodeSamples";
        private static readonly string storageAccountName = "kcddev";
        private static readonly string storageAccountKey = "";

        private readonly CloudTable fileCodeSamplesTable;

        public CodeSampleFileRepository()
        {
            var cloudStorageAccount = new CloudStorageAccount(new StorageCredentials(storageAccountName, storageAccountKey), true);
            var tableClient = cloudStorageAccount.CreateCloudTableClient();

            fileCodeSamplesTable = tableClient.GetTableReference(tableName);

            CreateTableIfNotExists().Wait();
        }

        public async Task<CodeSampleFile> ArchiveFileAsync(CodeSampleFile file)
        {
            var fileDto = getSerializedFileCodeSamples(file);

            return getDeserializedFileCodeSamples(await ExecuteTableOperation(TableOperation.Delete(fileDto)));
        }
        public async Task<CodeSampleFile> GetFileAsync(String filePath)
            => getDeserializedFileCodeSamples(await ExecuteTableOperation(TableOperation.Retrieve<CodeSampleFileDto>(filePath, filePath)));

        public async Task<CodeSampleFile> UpdateFileAsync(CodeSampleFile fileToUpdate)
        {
            var fileDto = getSerializedFileCodeSamples(fileToUpdate);

            return getDeserializedFileCodeSamples(await ExecuteTableOperation(TableOperation.Replace(fileDto)));
        }

        public async Task<CodeSampleFile> AddFileAsync(CodeSampleFile newFile)
        {
            var fileDto = getSerializedFileCodeSamples(newFile);
            var addedFile = await ExecuteTableOperation(TableOperation.InsertOrReplace(fileDto));

            return getDeserializedFileCodeSamples(addedFile); 
        }

        private async Task<CodeSampleFileDto> ExecuteTableOperation(TableOperation operation)
        {
            var executedOperation = await fileCodeSamplesTable.ExecuteAsync(operation);

            return executedOperation.Result as CodeSampleFileDto;
        }

        private async Task CreateTableIfNotExists()
        {
            if (!await fileCodeSamplesTable.ExistsAsync())
            {
                await fileCodeSamplesTable.CreateAsync();
            };
        }

        private CodeSampleFileDto getSerializedFileCodeSamples(CodeSampleFile file)
            => new CodeSampleFileDto() { FilePath = file.FilePath, CodeSamples = JsonConvert.SerializeObject(file.CodeSamples), ETag = "*" };

        private CodeSampleFile getDeserializedFileCodeSamples(CodeSampleFileDto fileDto)
            => new CodeSampleFile() { FilePath = fileDto.FilePath, CodeSamples = JsonConvert.DeserializeObject<List<CodeSample>>(fileDto.CodeSamples) };
    }
}
