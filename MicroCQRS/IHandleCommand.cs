using System.Threading.Tasks;

namespace MicroCQRS
{
    public interface ICommandHandler
    {
        IAggregateRepository AggregateRepository { get; set; }
    }

    public interface IHandleCommand<TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        Task HandlesAsync(TCommand command);
    }
}
