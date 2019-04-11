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

            await cloudBlobContainer.CreateIfNotExistsAsync();

            return new EventDataRepository { CloudBlobContainer = cloudBlobContainer };
        }

        public async Task StoreAsync(CodeFragmentEvent codeFragmentEvent)
        {
            var cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(Guid.NewGuid().ToString());
            var serializedCodeFragmentsEntity = JsonConvert.SerializeObject(codeFragmentEvent);

            await cloudBlockBlob.UploadTextAsync(serializedCodeFragmentsEntity);
        }
    }
}