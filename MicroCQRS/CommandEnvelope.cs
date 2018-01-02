using System;

namespace MicroCQRS.Azure
{
    public class CommandEnvelope
    {
        public CommandEnvelope()
        {
        }

        public CommandEnvelope(ICommand command)
        {
            Type = command.GetType();
            Command = command.Serialize();
        }

        public Type Type { get; set; }
        public string Command { get; set; }
    }
}
