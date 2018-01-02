using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroCQRS.Azure;

namespace MicroCQRS
{
    public class MicroCQRS
    {
        readonly IRepositoryProvider _repositoryProvider;
        readonly CommandBus _commandBus;
        readonly CommandProcessor _processor;
        
        MicroCQRS(
            IRepositoryProvider repositoryProvider,
            Func<Type, ICommandHandler> activator = null)
        {
            _repositoryProvider = repositoryProvider;
            var commandHandlers = CommandHandlerDiscovery.GetAllCommandHandlers(repositoryProvider.AggregateRepository, activator);
            var commandExecution = new CommandExecution(commandHandlers);
            _commandBus = new CommandBus(repositoryProvider.CommandRepository, repositoryProvider.CommandBusPersistence);
            _processor = new CommandProcessor(_commandBus, _repositoryProvider.CommandRepository, commandExecution);
        }

        public static MicroCQRS Build(IRepositoryProvider repositoryProvider, Func<Type, ICommandHandler> activator = null)
        {
            return new MicroCQRS(repositoryProvider, activator);
        }

        public async Task<Guid> SendAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            return await _commandBus.SendAsync(command);
        }

        public async Task<TAggregate> GetAggregateAsync<TAggregate>(Guid aggregateId) where TAggregate : Aggregate
        {
            return await _repositoryProvider.AggregateRepository.GetAsync<TAggregate>(aggregateId);
        }

        public async Task SaveAggregateAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            await _repositoryProvider.AggregateRepository.Save(aggregate);
        }

        public async Task<CommandAudit> GetCommandAuditByStatus(Guid aggregateId, Guid commandId, params CommandState[] states)
        {
            return await _repositoryProvider.CommandRepository.GetCommandAuditByStatus(aggregateId, commandId, states);
        }

        public async Task<IEnumerable<CommandAudit>> GetCommandAuditEntriesAsync(Guid aggregateId, Guid commandId)
        {
            return await _repositoryProvider.CommandRepository.GetAllCommandAuditsAsync(aggregateId, commandId);
        }

        public MicroCQRSServer BuildServer(ICommandProcessingScheduler scheduler)
        {
            return new MicroCQRSServer(scheduler, _commandBus, _processor, _repositoryProvider);
        }

        public MicroCQRSServer BuildServer<TCommandProcessingScheduler>() where TCommandProcessingScheduler : ICommandProcessingScheduler
        {
            return BuildServer((ICommandProcessingScheduler)Activator.CreateInstance(typeof(TCommandProcessingScheduler)));
        }
    }
}
