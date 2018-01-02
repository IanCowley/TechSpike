using System;
using System.Linq;
using MicroCQRS.Azure;
using Shouldly;

namespace MicroCQRS.Tests
{
    public class CommandTestsBase
    {
        protected void AssertHasCommandEntries<TCommand>(Guid aggregateId, Guid commandId,
            params (CommandState State, int Attempt)[] expectedStates) where TCommand : ICommand
        {
            var entries = TestContainer.GetCommandAuditEntries(aggregateId, commandId).OrderBy(x => x.Timestamp).ToArray();

            for (var index = 0; index < expectedStates.Length; index++)
            {

                entries[index].State.ShouldBe(expectedStates[index].State);
                entries[index].Attempt.ShouldBe(expectedStates[index].Attempt);
                entries[index].Command.GetType().ShouldBe(typeof(TCommand));
            }

            entries.Length.ShouldBe(expectedStates.Length, "Should only contain expected entries");
        }
    }
}