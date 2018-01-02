using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroCQRS.Azure;

namespace MicroCQRS
{
    public interface ICommandRepository
    {
        Task QueuedAsync<TCommand>(TCommand command) where TCommand : ICommand;
        Task StartingAsync(ICommand command, int attempt);
        Task ErroredAsync(ICommand command, int attempt);
        Task CompletedAsync(CommandAttempt commandAttempt);
        Task AbandonedAsync(CommandAttempt commandAttempt);
        Task HandlerNotFound(CommandAttempt commandAttempt);
        Task<CommandAudit> GetCommandAuditByStatus(Guid aggregateId, Guid commandId, params CommandState[] state);
        Task<IEnumerable<CommandAudit>> GetAllCommandAuditsAsync(Guid aggregateId, Guid commandId);
    }
}