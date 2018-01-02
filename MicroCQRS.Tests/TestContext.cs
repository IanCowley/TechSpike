using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MicroCQRS.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace MicroCQRS.Tests
{
    public class TestContext
    {
        readonly MicroCQRS _cqrs;
        readonly string _storageAccountName;
        readonly string _storageAccountKey;
        readonly BackOffRetryTimingStrategy _retryStrategy;
        readonly MicroCQRSServer _server;

        public TestContext(string testId, IRetryTimingStrategy retryTimingStrategy = null, Func<Type, ICommandHandler> activator = null)
        {
            _storageAccountName = Environment.GetEnvironmentVariable("MicroCQRSStorageAccount");
            _storageAccountKey = Environment.GetEnvironmentVariable("MicroCQRSStorageKey");
            _retryStrategy = new BackOffRetryTimingStrategy();

            _cqrs = AzureMicroCQRSFactory.Build(
                _storageAccountName, 
                _storageAccountKey,
                storagePrefix: testId,
                activator: activator);

            _server = _cqrs.BuildServer<SpinLoopScheduler>();
        }

        public void StartProcessing()
        {
            StartProcessing(retryStrategy: _retryStrategy);
        }

        public void StartProcessing(IRetryTimingStrategy retryStrategy)
        {
            _server.StartAsync(1, retryStrategy ?? _retryStrategy);
        }

        public void StopProcessing()
        {
            _server.Stop();
        }

        public async Task CleanUpAsync(string prefix = null)
        {
            var storageAccount = new CloudStorageAccount(new StorageCredentials(_storageAccountName, _storageAccountKey), true);

            var blobClient = storageAccount.CreateCloudBlobClient();
            BlobContinuationToken blobToken = null;

            do
            {
                var resultSegment = await blobClient.ListContainersSegmentedAsync(prefix, blobToken);
                blobToken = resultSegment.ContinuationToken;
                foreach (var container in resultSegment.Results)
                {
                    await container.DeleteIfExistsAsync();
                }
            } while (blobToken != null);

            var tableClient = storageAccount.CreateCloudTableClient();
            TableContinuationToken tableToken = null;

            do
            {
                var resultSegment = await tableClient.ListTablesSegmentedAsync(prefix, tableToken);
                tableToken = resultSegment.ContinuationToken;
                foreach (var table in resultSegment.Results)
                {
                    await table.DeleteIfExistsAsync();
                }
            } while (tableToken != null);


            var queueClient = storageAccount.CreateCloudQueueClient();
            QueueContinuationToken queueToken = null;

            do
            {
                var resultSegment = await queueClient.ListQueuesSegmentedAsync(prefix, queueToken);
                queueToken = resultSegment.ContinuationToken;
                foreach (var queue in resultSegment.Results)
                {
                    await queue.DeleteIfExistsAsync();
                }
            } while (queueToken != null);
        }

        public async Task<TAggregate> GetAggregateAsync<TAggregate>(Guid aggregateId) where TAggregate : Aggregate
        {
            return await _cqrs.GetAggregateAsync<TAggregate>(aggregateId);
        }

        public async Task SaveAggregateAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            await _cqrs.SaveAggregateAsync(aggregate);
        }

        public async Task<Guid> SendCommandAndWaitAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var commandId =  await _cqrs.SendAsync(command);

            while (true)
            {
                var commandAudit = await _cqrs
                    .GetCommandAuditByStatus(
                        command.AggregateId, 
                        command.Id,
                        CommandStateExtensions.Complete);

                if (commandAudit != null)
                {
                   return commandId;
                }
            }
        }

        public async Task<IEnumerable<CommandAudit>> GetCommandAuditEntriesAsync(Guid aggregateId, Guid commandId)
        {
            return await _cqrs.GetCommandAuditEntriesAsync(aggregateId, commandId);
        }

        public void SetRetryTimeInSeconds(int delay)
        {
            _retryStrategy.RetryBackOffInSeconds = delay;
        }
    }
}