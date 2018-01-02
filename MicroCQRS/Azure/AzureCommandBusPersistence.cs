using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MicroCQRS.Azure
{
    public class AzureCommandBusPersistence : ICommandBusPersistence
    {
        readonly AzureStorageProvider _storageProvider;
        readonly AzureCommandRepository _commandRepository;

        public AzureCommandBusPersistence(AzureStorageProvider storageProvider, AzureCommandRepository commandRepository)
        {
            _storageProvider = storageProvider;
            _commandRepository = commandRepository;
        }

        public async Task SendAsync<TCommand>(TCommand command, int delaySeconds = 0) where TCommand : ICommand
        {
            await _storageProvider.SaveQueueMessage(
                MicroCQRSAzureConstants.Commands.QueueName, 
                new CloudQueueMessage(new CommandEnvelope(command).Serialize()), delaySeconds);
        }

        public async Task ClearAsync(CommandAttempt commandAttempt) 
        {
            await _storageProvider.DeleteQueueMessageAsync(
                MicroCQRSAzureConstants.Commands.QueueName,
                commandAttempt.MessageId,
                commandAttempt.PopReceipt);
        }

        public async Task<CommandAttempt> GetNextCommandAttemptAsync()
        {
            var commandQueueMessage = await _storageProvider.GetQueuedMessageAsync(MicroCQRSAzureConstants.Commands.QueueName);

            if (commandQueueMessage != null)
            {
                var commandMessage = commandQueueMessage.DeserializeMessage();
                var command = commandMessage.DeserializeCommand();
                var commandAudit = await _commandRepository.GetCommandAuditByStatus(command.AggregateId, command.Id, CommandStateExtensions.All);

                var attempt = new CommandAttempt
                {
                    Command = command,
                    PopReceipt = commandQueueMessage.PopReceipt,
                    MessageId = commandQueueMessage.Id
                };

                if (commandAudit != null)
                {
                    attempt.CommandState = commandAudit.State;
                    attempt.Attempt = commandAudit.Attempt;
                }
                else
                {
                    attempt.CommandState = CommandState.Queued;
                    attempt.Attempt = 0;
                }

                return attempt;
            }

            return null;
        }
    }
}
