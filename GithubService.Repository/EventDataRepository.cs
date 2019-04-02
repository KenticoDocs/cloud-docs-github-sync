using System;
using System.Threading.Tasks;
using GithubService.Repository.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GithubService.Repository
{
    public class EventDataRepository : IEventDataRepository
    {
        private CloudBlobContainer CloudBlobContainer { get; set; }

        private EventDataRepository()
        {
        }

        public static async Task<EventDataRepository> CreateInstance(string connectionString)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("code-sample-fragments");
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };

            await cloudBlobContainer.CreateIfNotExistsAsync();
            await cloudBlobContainer.SetPermissionsAsync(permissions);

            var instance = new EventDataRepository {CloudBlobContainer = cloudBlobContainer};

            return instance;
        }

        public async Task StoreAsync(CodeFragmentsEntity codeFragments)
        {
            var cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(Guid.NewGuid().ToString());
            var serializedCodeFragmentsEntity = JsonConvert.SerializeObject(codeFragments);

            await cloudBlockBlob.UploadTextAsync(serializedCodeFragmentsEntity);
        }
    }
}