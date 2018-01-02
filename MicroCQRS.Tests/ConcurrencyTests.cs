using System;
using MicroCQRS.Azure;
using MicroCQRS.Tests.TestDomain.CommandHandlers;
using MicroCQRS.Tests.TestDomain.Commands;
using MicroCQRS.Tests.TestDomain.Model;
using NUnit.Framework;
using Shouldly;

namespace MicroCQRS.Tests
{
    [TestFixture]
    public class ConcurrencyTests
    {
        public class When_aggregate_mutated_outside_of_current_transaction : CommandTestsBase
        {
            Guid _aggregateId;
            Guid _commandId;

            [SetUp]
            public void Because_of_aggregate_changing_during_unit_of_work()
            {
                _aggregateId = Guid.NewGuid();

                TestContainer.SetRetryTimeInSeconds(1);
                TestContainer.SendCommandAndWait(new MadeSale(_aggregateId, 100));
                var alreadyUpdated = false;

                SalesCommandHandler.SalesCallBack = () =>
                {
                    if (alreadyUpdated)
                    {
                        return;
                    }

                    var aggregate = TestContainer.GetAggregate<Sales>(_aggregateId);
                    aggregate.AddSale(100);
                    TestContainer.SaveAggregateAsync(aggregate).Wait();
                    alreadyUpdated = true;
                };

                _commandId = TestContainer.SendCommandAndWait(new MadeSale(_aggregateId, 100));
            }

            [Test]
            public void It_should_retry_and_not_overwrite_previous_mutation()
            {
                TestContainer.GetAggregate<Sales>(_aggregateId).RunningTotal.ShouldBe(300);
            }

            [Test]
            public void It_should_not_have_scheduled()
            {
                AssertHasCommandEntries<MadeSale>(
                    _aggregateId,
                    _commandId,
                    (CommandState.Queued, 0),
                    (CommandState.Started, 1),
                    (CommandState.Completed, 1));
            }
        }
    }
}
