using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MicroCQRS.Azure
{
    public class AzureCommandRepository : ICommandRepository
    {
        readonly AzureStorageProvider _storageProvider;

        public AzureCommandRepository(AzureStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }
        
        public async Task QueuedAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            await SaveCommandAuditAsync(command, CommandState.Queued);
        }

        async Task DeleteQueueItemAsync(string messageId, string popReceipt)
        {
            await _storageProvider.DeleteQueueMessageAsync(MicroCQRSAzureConstants.Commands.QueueName, messageId, popReceipt);
        }

        public async Task StartingAsync(ICommand command, int attempt)
        {
            await SaveCommandAuditAsync(command, CommandState.Started, attempt);
        }

        public async Task ErroredAsync(ICommand command, int attempt)
        {
            await SaveCommandAuditAsync(command, CommandState.Errored, attempt);
        }

        public async Task CompletedAsync(CommandAttempt commandAttempt)
        {
            await SaveCommandAuditAsync(commandAttempt.Command, CommandState.Completed, commandAttempt.Attempt);
            await DeleteQueueItemAsync(commandAttempt.MessageId, commandAttempt.PopReceipt);
        }

        public async Task HandlerNotFound(CommandAttempt commandAttempt)
        {
            await SaveCommandAuditAsync(commandAttempt.Command, CommandState.HandlerNotFound, commandAttempt.Attempt);
            await DeleteQueueItemAsync(commandAttempt.MessageId, commandAttempt.PopReceipt);
        }

        public async Task AbandonedAsync(CommandAttempt commandAttempt)
        {
            await SaveCommandAuditAsync(commandAttempt.Command, CommandState.Abandoned, commandAttempt.Attempt);
            await DeleteQueueItemAsync(commandAttempt.MessageId, commandAttempt.PopReceipt);
        }

        public async Task<CommandAudit> GetCommandAuditByStatus(Guid aggregateId, Guid commandId, params CommandState[] states)
        {
            var query = new TableQuery<CommandAuditEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, aggregateId.ToString()));

            var auditEntities = await _storageProvider.Get(MicroCQRSAzureConstants.Commands.TableName, query);
            var commandIdString = commandId.ToString();
            var entity = auditEntities
                .OrderBy(x => x.Timestamp)
                .LastOrDefault(x => x.CommandId == commandIdString && states.Contains((CommandState)x.State));

            if (entity == null)
            {
                return null;
            }

            return new CommandAudit(entity);
        }

        public async Task<IEnumerable<CommandAudit>> GetAllCommandAuditsAsync(Guid aggregateId, Guid commandId)
        {
            var query = new TableQuery<CommandAuditEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, aggregateId.ToString()));

            var auditEntries = await _storageProvider.Get(MicroCQRSAzureConstants.Commands.TableName, query);
            var commandIdString = commandId.ToString();
            return auditEntries.Where(x => x.CommandId == commandIdString).Select(x => new CommandAudit(x));
        }

        async Task SaveCommandAuditAsync(ICommand command, CommandState state, int attempt = 0)
        {
            await _storageProvider.SaveTableAsync(
                MicroCQRSAzureConstants.Commands.TableName,
                new CommandAuditEntity(command.AggregateId, Guid.NewGuid())
                {
                    CommandId  = command.Id.ToString(),
                    Content = command.Serialize(),
                    State = (int)state,
                    Attempt = attempt,
                    Type = command.GetType().AssemblyQualifiedName
                });
        }
    }

    public class CommandAudit
    {
        public CommandAudit(CommandAuditEntity entity)
        {
            Command = entity.DeserializeCommand();
            State = (CommandState) entity.State;
            Attempt = entity.Attempt;
            Timestamp = entity.Timestamp;
        }
        
        public object Command { get; set; }
        public CommandState State { get; set; }
        public int Attempt { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public override string ToString()
        {
            return $"{State} - {Attempt}";
        }
    }
}