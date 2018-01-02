using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroCQRS.Azure;

namespace MicroCQRS.Tests.InMemoryCommands
{
    public class InMemoryRepositoryProvider : IRepositoryProvider
    {
        readonly InMemoryAggregateRepository _aggregateRepository;
        readonly InMemoryCommandRepository _commandRepository;
        readonly InMemoryCommandBusPersistence _commandBusPersistence;

        public InMemoryRepositoryProvider()
        {
            _aggregateRepository = new InMemoryAggregateRepository();
            _commandRepository = new InMemoryCommandRepository();
            _commandBusPersistence = new InMemoryCommandBusPersistence();
        }

        public IAggregateRepository AggregateRepository => _aggregateRepository;
        public ICommandRepository CommandRepository => _commandRepository;
        public ICommandBusPersistence CommandBusPersistence => _commandBusPersistence;

        public Task BootstrapAsync()
        {
            return Task.FromResult(0);
        }
    }

    public class InMemoryCommandRepository : ICommandRepository
    {
        public Task QueuedAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            throw new NotImplementedException();
        }

        public Task StartingAsync(ICommand command, int attempt)
        {
            throw new NotImplementedException();
        }

        public Task ErroredAsync(ICommand command, int attempt)
        {
            throw new NotImplementedException();
        }

        public Task CompletedAsync(CommandAttempt commandAttempt)
        {
            throw new NotImplementedException();
        }

        public Task AbandonedAsync(CommandAttempt commandAttempt)
        {
            throw new NotImplementedException();
        }

        public Task HandlerNotFound(CommandAttempt commandAttempt)
        {
            throw new NotImplementedException();
        }

        public Task<CommandAudit> GetCommandAuditByStatus(Guid aggregateId, Guid commandId, params CommandState[] state)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CommandAudit>> GetAllCommandAuditsAsync(Guid aggregateId, Guid commandId)
        {
            throw new NotImplementedException();
        }

        public Task<CommandAudit> GetCommandAuditByStatus(Guid aggregateId, Guid commandId)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryCommandBusPersistence : ICommandBusPersistence
    {
        public Task SendAsync<TCommand>(TCommand command, int delaySeconds) where TCommand : ICommand
        {
            throw new NotImplementedException();
        }

        public Task ClearAsync(CommandAttempt commandAttempt)
        {
            throw new NotImplementedException();
        }

        public Task<CommandAttempt> GetNextCommandAttemptAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryAggregateRepository : IAggregateRepository
    {
        public Task<TAggregate> GetOrCreateAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            throw new NotImplementedException();
        }

        public Task<TAggregate> GetAsync<TAggregate>(Guid aggregateId) where TAggregate : Aggregate
        {
            throw new NotImplementedException();
        }

        public Task Save<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            throw new NotImplementedException();
        }
    }
}
