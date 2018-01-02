using System.Reflection;
using System.Threading.Tasks;

namespace MicroCQRS
{
    public class CommandHandlerMap
    {
        readonly ICommandHandler _commandHandler;
        readonly MethodInfo _handlesMethod;

        public CommandHandlerMap(ICommandHandler commandHandler, MethodInfo handlesMethod)
        {
            _commandHandler = commandHandler;
            _handlesMethod = handlesMethod;
        }

        public async Task Execute(ICommand command)
        {
            await (Task)_handlesMethod.Invoke(_commandHandler, new []{ command });
        }
    }
}