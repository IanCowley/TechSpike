using System;
using System.Threading.Tasks;

namespace MicroCQRS
{
    public interface IAggregateRepository
    {
        Task<TAggregate> GetOrCreateAsync<TAggregate>(Guid id) where TAggregate : Aggregate;
        Task<TAggregate> GetAsync<TAggregate>(Guid aggregateId) where TAggregate : Aggregate;
        Task Save<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate;
    }
}