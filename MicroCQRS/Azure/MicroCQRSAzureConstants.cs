using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroCQRS.Azure
{
    public static class MicroCQRSAzureConstants
    {
        public static IEnumerable<string> Tables => new[] { Commands.TableName };
        public static IEnumerable<string> Queues => new[] { Commands.TableName };
        public static IEnumerable<string> BlobContainers => new[] { Aggregates.ContainerName };

        public static class Aggregates
        {
            public const string ContainerName = "mcqrs-aggregates";
        }

        public static class Commands
        {
            public const string TableName = "commands";
            public const string QueueName = "commands";
        }
    }

    public enum CommandState
    {
        Queued,
        Started,
        Errored,
        Completed,
        Abandoned,
        HandlerNotFound
    }

    public static class CommandStateExtensions
    {
        public static CommandState[] All = {CommandState.Queued, CommandState.Started, CommandState.Completed, CommandState.Errored, CommandState.Abandoned, CommandState.HandlerNotFound};

        public static CommandState[] Complete = { CommandState.Completed, CommandState.Abandoned, CommandState.HandlerNotFound };

        public static bool IsComplete(this CommandState state) => Complete.Contains(state);
    }
}
