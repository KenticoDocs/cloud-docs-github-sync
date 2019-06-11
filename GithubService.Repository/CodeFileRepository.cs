using GithubService.Models;
using GithubService.Repository.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CodeFragment = GithubService.Models.CodeFragment;

namespace GithubService.Repository
{
    public class CodeFileRepository : ICodeFileRepository
    {
        private const string EntityRowKey = "cf";

        private readonly CloudTable _codeFileTable;

        public CodeFileRepository(CloudTable codeFileTable)
        {
            _codeFileTable = codeFileTable;
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
            var retrievedResult = await _codeFileTable.ExecuteAsync(operation);

            return (CodeFileEntity)retrievedResult.Result;
        }

        private async Task<CodeFileEntity> StoreEntityAsync(CodeFileEntity entity)
        {
            var operation = TableOperation.InsertOrReplace(entity);
            var result = await _codeFileTable.ExecuteAsync(operation);

            return (CodeFileEntity)result.Result;
        }

        private string ConstructPartitionKey(string filePath)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(filePath));

            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}
