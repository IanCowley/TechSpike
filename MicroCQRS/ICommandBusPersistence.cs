using System;
using System.Threading.Tasks;
using MicroCQRS.Azure;

namespace MicroCQRS
{
    public interface ICommandBusPersistence
    {
        Task SendAsync<TCommand>(TCommand command, int delaySeconds = 0) where TCommand : ICommand;
        Task ClearAsync(CommandAttempt commandAttempt);
        Task<CommandAttempt> GetNextCommandAttemptAsync();
    }
}
