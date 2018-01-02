using System.Threading;
using System.Threading.Tasks;
using MicroCQRS.Azure;

namespace MicroCQRS
{
    public class MicroCQRSServer
    {
        readonly ICommandProcessingScheduler _scheduler;
        readonly CommandBus _commandBus;
        readonly CommandProcessor _processor;
        readonly IRepositoryProvider _repositoryProvider;
        IRetryTimingStrategy _retryTimingStrategy;
        CancellationTokenSource _cancellationTokenSource;

        internal MicroCQRSServer(
            ICommandProcessingScheduler scheduler, 
            CommandBus commandBus, 
            CommandProcessor processor, 
            IRepositoryProvider repositoryProvider)
        {
            _scheduler = scheduler;
            _commandBus = commandBus;
            _processor = processor;
            _repositoryProvider = repositoryProvider;
        }
        
        public async Task StartAsync(int maxParallalisation = 1, IRetryTimingStrategy retryTimingStrategy = null)
        {
            await _repositoryProvider.BootstrapAsync();
            _retryTimingStrategy = retryTimingStrategy ?? new BackOffRetryTimingStrategy();
            _cancellationTokenSource = new CancellationTokenSource();
            await _scheduler.Start(maxParallalisation, ProcessAsync, _cancellationTokenSource.Token);
        }

        async Task ProcessAsync()
        {
            CommandAttempt commandAttempt;

            while (!_cancellationTokenSource.Token.IsCancellationRequested &&
                    (commandAttempt = await _commandBus.GetNextCommandAttemptAsync()) != null)
            {
                await _processor.ProcessQueueAsync(commandAttempt, _cancellationTokenSource.Token, _retryTimingStrategy);
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _scheduler.Stop();
        }
    }
}