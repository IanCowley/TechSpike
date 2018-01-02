using System;
using System.Threading.Tasks;
using log4net;
using MicroCQRS.Azure;
using Polly;
using RetryPolicy = Polly.Retry.RetryPolicy;

namespace MicroCQRS
{
    public abstract class CommandHandlerBase<TAggregate> where TAggregate : Aggregate
    {
        static readonly ILog Logger = LogManager.GetLogger(typeof(CommandHandlerBase<TAggregate>));
        readonly RetryPolicy _concurrencyPolicy;
        public IAggregateRepository AggregateRepository { get; set; }

        protected CommandHandlerBase()
        {
            _concurrencyPolicy = Policy
                .Handle<MicroCQRSConcurrentyException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Logger.Warn($"Concurrency error executing command {exception}, retry count {retryCount}");
                    });
        }

        protected async Task PerformUnitOfWorkAsync<TCommand>(TCommand command, Action<TAggregate> unitOfWork) 
            where TCommand : ICommand
        {
            await _concurrencyPolicy.ExecuteAsync(async () =>
            {
                var aggregate = await AggregateRepository.GetOrCreateAsync<TAggregate>(command.AggregateId);
                aggregate.Id = command.AggregateId;
                unitOfWork(aggregate);
                await AggregateRepository.Save(aggregate);
            });
        }
    }
}
