using System.Threading.Tasks;

namespace MicroCQRS
{
    public interface IRepositoryProvider
    {
        IAggregateRepository AggregateRepository { get; }
        ICommandRepository CommandRepository { get; }
        ICommandBusPersistence CommandBusPersistence { get; }
        Task BootstrapAsync();
    }
}
