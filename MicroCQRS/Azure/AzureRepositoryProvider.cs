using System.Threading.Tasks;

namespace MicroCQRS.Azure
{
    public class AzureRepositoryProvider : IRepositoryProvider
    {
        readonly AzureStorageProvider _storageProvider;
        bool _bootStrapped;

        public AzureRepositoryProvider(string storageAccountName, string storageAccountKey, string prefix)
        {
            _storageProvider = new AzureStorageProvider(storageAccountName, storageAccountKey, prefix);
            AggregateRepository = new AzureAggregateRepository(_storageProvider);
            CommandRepository = new AzureCommandRepository(_storageProvider);
            CommandBusPersistence = new AzureCommandBusPersistence(_storageProvider, (AzureCommandRepository)CommandRepository);
        }

        public IAggregateRepository AggregateRepository { get; }
        public ICommandRepository CommandRepository { get; }
        public ICommandBusPersistence CommandBusPersistence { get; set; }

        public async Task BootstrapAsync()
        {
            if (_bootStrapped)
            {
                return;
            }
            
            foreach (var blobContainer in MicroCQRSAzureConstants.BlobContainers)
            {
                await _storageProvider.EnsureContainerExists(blobContainer);
            }

            foreach (var queue in MicroCQRSAzureConstants.Queues)
            {
                await _storageProvider.EnsureQueueExists(queue);
            }

            foreach (var table in MicroCQRSAzureConstants.Tables)
            {
                await _storageProvider.EnsureTableExists(table);
            }

            _bootStrapped = true;
        }
    }
}
