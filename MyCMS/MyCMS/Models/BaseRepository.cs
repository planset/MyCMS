using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace MyCMS.Models
{

    public abstract class BaseRepository<TEntity>
    {
        private CloudStorageAccount _StorageAccount;

        protected CloudTableClient Client { get; }
        protected CloudTable Table { get; }
        protected string TableName {
            get {
                return RepositoryHelpers.GetTableName(typeof(TEntity));
            }
        }

        public BaseRepository()
        {
            var connectionString = CurrentConfiguration.Configuration.GetConnectionString("AzureStorage");
            this._StorageAccount = CloudStorageAccount.Parse(connectionString);
            this.Client = this._StorageAccount.CreateCloudTableClient();
            this.Table = this.Client.GetTableReference(this.TableName);
            AsyncHelpers.RunSync(() => this.CreateTable());
        }

        public async Task CreateTable()
        {
            await this.Table.CreateIfNotExistsAsync();
        }

    }
}
