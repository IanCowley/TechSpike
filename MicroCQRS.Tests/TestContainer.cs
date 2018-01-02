using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroCQRS.Azure;

namespace MicroCQRS.Tests
{
    public static class TestContainer
    {
        static string _testId;
        const string testPrefix = "microtest";
        public static TestContext Current { get; set; }

        public static void Initialise(Func<Type, ICommandHandler> activator = null)
        {
            Current = NewContext(activator);
            Current.StartProcessing();
        }

        public static TestContext NewContext(Func<Type, ICommandHandler> activator = null)
        {
            _testId = $"{testPrefix}{Guid.NewGuid():n}";
            var context = new TestContext(_testId, activator: activator);
            return context;
        }

        public static void Stop()
        {
            Current.StopProcessing();
        }

        public static void CleanUp()
        {
            Current.CleanUpAsync(_testId).Wait();
        }

        public static void CleanUpAll()
        {
            Current.CleanUpAsync(testPrefix).Wait();
        }

        public static TAggregate GetAggregate<TAggregate>(Guid aggregateId) where TAggregate : Aggregate
        {
            return Current.GetAggregateAsync<TAggregate>(aggregateId).Result;
        }

        public static Guid SendCommandAndWait<TCommand>(TCommand command) where TCommand : ICommand
        {
            return Current.SendCommandAndWaitAsync(command).Result;
        }

        public static IEnumerable<CommandAudit> GetCommandAuditEntries(Guid aggregateId, Guid commandId)
        {
            return Current.GetCommandAuditEntriesAsync(aggregateId, commandId).Result;
        }

        public static void SetRetryTimeInSeconds(int delay)
        {
            Current.SetRetryTimeInSeconds(delay);
        }

        public static async Task SaveAggregateAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            await Current.SaveAggregateAsync(aggregate);
        }
    }
}
