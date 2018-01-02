using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Polly;
using Polly.Wrap;

namespace MicroCQRS.Azure
{
    public class AzureStorageProvider
    {
        static readonly ILog Logger = LogManager.GetLogger(typeof(AzureStorageProvider));
        readonly string _storageAccountName;
        readonly string _storageAccountKey;
        readonly string _prefix;
        readonly PolicyWrap _policy;
        const int QueueProcessingDelaySeconds = 60;

        public AzureStorageProvider(string storageAccountName, string storageAccountKey, string prefix = null)
        {
            _storageAccountName = storageAccountName;
            _storageAccountKey = storageAccountKey;
            _prefix = prefix;

            _policy =
        }

        public CloudBlockBlob GetBlobReference(string containerName, string blobName)
        {
            return GetContainerReference(containerName).GetBlockBlobReference(blobName);
        }

        public async Task SaveTableAsync<TEntity>(string tableName, TEntity entity) where TEntity : TableEntity
        {
            var table = GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            var insert = TableOperation.Insert(entity);
            await table.ExecuteAsyncIgnore409(insert);
        }

        public async Task<IEnumerable<TEntity>> Get<TEntity>(string tableName, TableQuery<TEntity> query) where TEntity : TableEntity, new()
        {
            var table = GetTableReference(tableName);
            var allEntities = new List<TEntity>();

            TableContinuationToken tableContinuationToken = null;
            do
            {
                var queryResponse = await table.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
                tableContinuationToken = queryResponse.ContinuationToken;
                allEntities.AddRange(queryResponse.Results);
            }
            while (tableContinuationToken != null);

            return allEntities.OrderByDescending(x => x.Timestamp);
        }

        public async Task SaveQueueMessage(string queueName, CloudQueueMessage message, int delaySeconds = 0)
        {
            var queue = GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            await queue.AddMessageAsync(
                message: message,
                initialVisibilityDelay: TimeSpan.FromSeconds(delaySeconds),
                timeToLive:null,
                options:null,
                operationContext:null);
        }

        public async Task<CloudQueueMessage> GetQueuedMessageAsync(string queueName)
        {
            var messages = await GetQueueReference(queueName).GetMessagesAsync(1, TimeSpan.FromSeconds(QueueProcessingDelaySeconds), null, null);
            return messages.FirstOrDefault();
        }

        public async Task DeleteQueueMessageAsync(string queueName, string messageId, string popReceipt)
        {
            await GetQueueReference(queueName).DeleteMessageAsync(messageId, popReceipt);
        }

        public async Task EnsureContainerExists(string containerName)
        {
            await GetContainerReference(containerName).CreateIfNotExistsAsync();
        }

        public async Task EnsureQueueExists(string queueName)
        {
            await GetQueueReference(queueName).CreateIfNotExistsAsync();
        }

        public async Task EnsureTableExists(string tableName)
        {
            await GetTableReference(tableName).CreateIfNotExistsAsync();
        }

        string AttachPrefix(string name)
        {
            return _prefix == null ? name : $"{_prefix}{name}";
        }

        CloudBlobContainer GetContainerReference(string containerName)
        {
            return BlobClient.GetContainerReference(AttachPrefix(containerName));
        }

        CloudBlobClient BlobClient
        {
            get
            {
                var blobClient = new CloudStorageAccount(new StorageCredentials(_storageAccountName, _storageAccountKey), true)
                    .CreateCloudBlobClient();

                blobClient.DefaultRequestOptions.RetryPolicy = new RetryPolicy().Create();
                return blobClient;
            }
        }

        CloudTable GetTableReference(string tableName)
        {
            return TableClient.GetTableReference(AttachPrefix(tableName));
        }

        CloudTableClient TableClient
        {
            get
            {
                var tableClient = new CloudStorageAccount(new StorageCredentials(_storageAccountName, _storageAccountKey), true)
                    .CreateCloudTableClient();

                tableClient.DefaultRequestOptions.RetryPolicy = new RetryPolicy().Create();
                return tableClient;
            }
        }

        CloudQueue GetQueueReference(string queueName)
        {
            return QueueClient.GetQueueReference(AttachPrefix(queueName));
        }

        CloudQueueClient QueueClient
        {
            get
            {
                var queueClient = new CloudStorageAccount(new StorageCredentials(_storageAccountName, _storageAccountKey), true)
                    .CreateCloudQueueClient();

                queueClient.DefaultRequestOptions.RetryPolicy = new RetryPolicy().Create();
                return queueClient;
            }
        }
    }

    public class RetryPolicy
    {
        public RetryPolicy()
        {
            DeltaBackoff = TimeSpan.FromSeconds(4);
            MaxAttempts = 16;
        }

        public int MaxAttempts { get; set; }
        public TimeSpan DeltaBackoff { get; set; }

        public IRetryPolicy Create()
        {
            return new ExponentialRetry(DeltaBackoff, MaxAttempts);
        }
    }
}
