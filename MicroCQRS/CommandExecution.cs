using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace MicroCQRS
{

    internal class CommandExecution
    {
        static readonly ILog Logger = LogManager.GetLogger(typeof(CommandProcessor));
        readonly Dictionary<Type, CommandHandlerMap> _commandHandlerMaps;

        public CommandExecution(Dictionary<Type, CommandHandlerMap> commandHandlerMaps)
        {
            _commandHandlerMaps = commandHandlerMaps;
        }

        public async Task ExecuteAsync(ICommand command, int attemptNumber, CancellationToken cancellationToken) 
        {
            try
            {
                Logger.Debug($"Executing command {command.Id} - {command.GetType()} attempt {attemptNumber}");
                var commandHandler = GetCommandHandler(command);

                if (commandHandler == null)
                {
                    throw new MicroCQRSConfigurationException(
                        $"There was no IHandleCommand for command {command.GetType()}");
                }

                await Task.Run(() => commandHandler.Execute(command), cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MicroCQRSCommandHandlerException(command.GetType(), command.AggregateId, ex, attemptNumber);
            }
        }

        public bool HandlerExists<TCommand>(TCommand command) where TCommand : ICommand
        {
            return _commandHandlerMaps.ContainsKey(command.GetType());
        }

        CommandHandlerMap GetCommandHandler(ICommand command) 
        {
           return _commandHandlerMaps[command.GetType()];
        }
    }
}
