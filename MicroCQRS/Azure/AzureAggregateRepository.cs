using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MicroCQRS.Azure
{
    public class AzureAggregateRepository : IAggregateRepository
    {
        readonly AzureStorageProvider _storageProvider;
        readonly JsonSerializer _serializer;

        public AzureAggregateRepository(AzureStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
            _serializer = new JsonSerializer();
        }

        public async Task<TAggregate> GetOrCreateAsync<TAggregate>(Guid id) where  TAggregate : Aggregate
        {
            try
            {
                var aggregate = await GetAsync<TAggregate>(id);

                if (aggregate != null)
                {
                    return aggregate;
                }

                aggregate = Activator.CreateInstance<TAggregate>();
                aggregate.Id = id;
                aggregate.Version = null;

                return aggregate;
            }
            catch (Exception ex)
            {
                throw new MicroCQRSException($"exception occurred getting aggregate {typeof(TAggregate)} with id {id}", ex);
            }
        }

        public async Task Save<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            try
            {
                var blob = _storageProvider.GetBlobReference(MicroCQRSAzureConstants.Aggregates.ContainerName, aggregate.Id.ToString());

                using (var stream = await blob.OpenWriteAsync(AccessCondition.GenerateIfMatchCondition(aggregate.Version),new BlobRequestOptions(),new OperationContext()))
                using (var jsonWriter = new JsonTextWriter(new StreamWriter(stream)))
                {
                    _serializer.Serialize(jsonWriter, aggregate);
                    await stream.FlushAsync(); 
                }
            }
            catch(StorageException sex)
            {
                if (sex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    throw new MicroCQRSConcurrentyException(sex);
                }

                throw;
            }
            catch (Exception ex)
            {
                throw new MicroCQRSException($"Error saving aggregate {typeof(TAggregate)} {aggregate.Id}", ex);
            }
        }

        public async Task<TAggregate> GetAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            try
            {
                var blob = _storageProvider.GetBlobReference(MicroCQRSAzureConstants.Aggregates.ContainerName,
                    id.ToString());

                if (!await blob.ExistsAsync())
                {
                    return null;
                }

                var aggregate = JsonConvert.DeserializeObject<TAggregate>(await blob.DownloadTextAsync());
                aggregate.Version = blob.Properties.ETag;
                return aggregate;
            }
            catch (Exception ex)
            {
                throw new MicroCQRSException($"exception occurred getting aggregate {typeof(TAggregate)} with id {id}", ex);
            }
        }
    }
}