using GithubService.Models.CodeSamples;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public async Task<CodeSampleFile> ArchiveFileAsync(CodeSampleFile file)
        {
            var fileDto = getSerializedFileCodeSamples(file);

            return getDeserializedFileCodeSamples(await ExecuteTableOperation(TableOperation.Delete(fileDto)));
        }

        public async Task<CodeSampleFile> GetFileAsync(string filePath)
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
            var executedOperation = await _fileCodeSamplesTable.ExecuteAsync(operation);

            return executedOperation.Result as CodeSampleFileDto;
        }

        private CodeSampleFileDto getSerializedFileCodeSamples(CodeSampleFile file)
            => new CodeSampleFileDto
            {
                FilePath = file.FilePath,
                CodeSamples = JsonConvert.SerializeObject(file.CodeSamples),
                ETag = "*"
            };

        private CodeSampleFile getDeserializedFileCodeSamples(CodeSampleFileDto fileDto)
            => new CodeSampleFile
            {
                FilePath = fileDto.FilePath,
                CodeSamples = JsonConvert.DeserializeObject<List<CodeSample>>(fileDto.CodeSamples)
            };
    }
}
