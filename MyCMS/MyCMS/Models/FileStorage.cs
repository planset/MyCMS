using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace MyCMS.Models
{
    public class FileStorage: IFileStorage
    {
        private CloudStorageAccount _StorageAccount;

        protected CloudBlobClient Client { get; }
        protected CloudBlobContainer Container{ get; }

        public FileStorage()
        {
            var connectionString = CurrentConfiguration.Configuration.GetConnectionString("AzureStorage");
            this._StorageAccount = CloudStorageAccount.Parse(connectionString);
            this.Client = this._StorageAccount.CreateCloudBlobClient();

            var blogConf = CurrentConfiguration.Configuration.GetSection("Blog");
            var containerName = blogConf.GetSection("ContentContainer").Value;
            this.Container = this.Client.GetContainerReference(containerName);

            AsyncHelpers.RunSync(() => Container.CreateIfNotExistsAsync());
            AsyncHelpers.RunSync(() => 
                this.Container.SetPermissionsAsync(
                    new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob }
                    )
            );
        }

        public async Task<FileInfo> AddAsync(string fileName, Stream stream)
        {
            // Retrieve reference to a blob named "myblob".
            var blockBlob = this.Container.GetBlockBlobReference(fileName);

            if (await blockBlob.ExistsAsync())
            {
                return new FileInfo()
                {
                    LinkUrl = blockBlob.Uri.ToString()
                };
            }
            //await blockBlob.DeleteIfExistsAsync();

            await blockBlob.UploadFromStreamAsync(stream);

            return new FileInfo()
            {
                LinkUrl = blockBlob.Uri.ToString()
            };
        }
    }
}
