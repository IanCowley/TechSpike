using System;
using System.Reflection;
using MicroCQRS.Azure;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace MicroCQRS
{
    public static class CommandExtensions
    {
        public static string Serialize(this CommandEnvelope commandEnvelope)
        {
            return JsonConvert.SerializeObject(commandEnvelope);
        }

        public static CommandEnvelope DeserializeMessage(this CloudQueueMessage message)
        {
            return JsonConvert.DeserializeObject<CommandEnvelope>(message.AsString);
        }

        public static string Serialize(this ICommand command)
        {
            return JsonConvert.SerializeObject(command);
        }

        public static ICommand DeserializeCommand(this CommandEnvelope commandEnvelope)
        {
            return (ICommand)JsonConvert.DeserializeObject(commandEnvelope.Command, commandEnvelope.Type);
        }

        public static object DeserializeCommand(this CommandAuditEntity auditEntity)
        {
            return JsonConvert.DeserializeObject(auditEntity.Content, Type.GetType(auditEntity.Type));
        }
    }
}
