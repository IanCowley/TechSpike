using System.Threading;
using System.Threading.Tasks;
using log4net;
using MicroCQRS.Azure;

namespace MicroCQRS
{
    internal class CommandProcessor
    {
        static readonly ILog Logger = LogManager.GetLogger(typeof(CommandProcessor));
        readonly CommandBus _commandBus;
        readonly ICommandRepository _commandRepository;
        readonly CommandExecution _commandExecution;
        readonly int _queuePollingIntervalMilliseconds;
        
        CancellationTokenSource _cancellationSource;
        const int MaxRetryAttempts = 5;

        public CommandProcessor(
            CommandBus commandBus,
            ICommandRepository commandRepository,
            CommandExecution commandExecution)
        {
            _commandBus = commandBus;
            _commandRepository = commandRepository;
            _commandExecution = commandExecution;
            
        }

       public async Task ProcessQueueAsync(
           CommandAttempt commandAttempt, 
           CancellationToken cancellationToken, 
           IRetryTimingStrategy retryStrategy)
        {
            try
            {
                commandAttempt.Attempt++;

                if (!_commandExecution.HandlerExists(commandAttempt.Command))
                {
                    await HandleCommandHandlerNotFoundAsync(commandAttempt);
                    return;
                }

                await _commandRepository.StartingAsync(commandAttempt.Command, commandAttempt.Attempt);
                await _commandExecution.ExecuteAsync(commandAttempt.Command, commandAttempt.Attempt, cancellationToken);
                await _commandRepository.CompletedAsync(commandAttempt);
            }
            catch (MicroCQRSCommandHandlerException ex)
            {
                await _commandRepository.ErroredAsync(commandAttempt.Command, commandAttempt.Attempt);
                await HandleExceptionAsync(ex, commandAttempt, retryStrategy);
            }
        }

        async Task HandleCommandHandlerNotFoundAsync(CommandAttempt commandAttempt)
        {
            await _commandRepository.HandlerNotFound(commandAttempt);
        }

        async Task HandleExceptionAsync(
            MicroCQRSCommandHandlerException exception, 
            CommandAttempt commandAttempt,
            IRetryTimingStrategy retryStrategy)
        {
            Logger.Error($"Aggregate {exception.CommandAggregateId} and command {exception.AttemptNumber} experienced exception", exception.Exception);

            if (exception.AttemptNumber >= MaxRetryAttempts)
            {
                Logger.Error($"Aggregate {exception.CommandAggregateId} and command {exception.AttemptNumber} exceeded the max retry police ({MaxRetryAttempts})");
                await _commandRepository.AbandonedAsync(commandAttempt);
            }
            else
            {
                Logger.Debug($"Re-queueing attmpt, exception was {exception}");
                await RequeueAttemptAsync(commandAttempt, retryStrategy);
            }
        }
        
        async Task RequeueAttemptAsync(CommandAttempt commandAttempt, IRetryTimingStrategy retryStrategy)
        {
            await _commandBus.SendRetryAsync(commandAttempt.Command, retryStrategy.GetNumberOfSecondsToWaitForNextAttempt(commandAttempt.Attempt));
            await _commandBus.ClearAsync(commandAttempt);
        }
    }
}
