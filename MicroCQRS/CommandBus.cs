using System;
using System.Threading.Tasks;
using MicroCQRS.Azure;

namespace MicroCQRS
{
    internal class CommandBus
    {
        readonly ICommandRepository _commandRepository;
        readonly ICommandBusPersistence _commandBusPersistence;

        public CommandBus(ICommandRepository commandRepository, ICommandBusPersistence commandBusPersistence)
        {
            _commandRepository = commandRepository;
            _commandBusPersistence = commandBusPersistence;
        }

        public async Task<Guid> SendAsync<TCommand>(TCommand command, int delaySeconds = 0) where TCommand : ICommand
        {
            if (command.Id == Guid.Empty)
            {
                command.Id = Guid.NewGuid();
            }

            await _commandBusPersistence.SendAsync(command, delaySeconds);
            await _commandRepository.QueuedAsync(command);
            return command.Id;
        }

        public async Task SendRetryAsync<TCommand>(TCommand command, int delaySeconds = 0) where TCommand : ICommand
        {
            await _commandBusPersistence.SendAsync(command, delaySeconds);
        }

        public async Task<CommandAttempt> GetNextCommandAttemptAsync()
        {
            return await _commandBusPersistence.GetNextCommandAttemptAsync();
        }

        public async Task ClearAsync(CommandAttempt commandAttempt)
        {
            await _commandBusPersistence.ClearAsync(commandAttempt);
        }
    }
}