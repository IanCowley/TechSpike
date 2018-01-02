using System;
using System.Linq;
using MicroCQRS.Azure;
using MicroCQRS.Tests.TestDomain.CommandHandlers;
using MicroCQRS.Tests.TestDomain.Commands;
using MicroCQRS.Tests.TestDomain.Model;
using NUnit.Framework;
using Shouldly;

namespace MicroCQRS.Tests
{
    public class When_command_handler_raises_errors_and_succeeds : CommandTestsBase
    {
        Guid _aggregateId;
        Guid _commandId;

        [Test]
        public void should_have_mutated_domain()
        {
            BecauseOfIssuingCommands(retryTimeInSeconds:1);

            var shop = TestContainer.GetAggregate<Shop>(_aggregateId);
            shop.Id.ShouldBe(_aggregateId);
            shop.Name.ShouldBe("Elmore groceries");
            shop.City.ShouldBe("Elmore");
            shop.OpeningDate.ShouldBe(new DateTime(1980, 5, 20));
            shop.Employees.Count.ShouldBe(1);

            var employee = shop.Employees.First();
            employee.FirstName.ShouldBe("Gumball");
            employee.LastName.ShouldBe("Watson");
            employee.Position.ShouldBe("Shop assistant");
        }

        [Test]
        public void Should_have_retried_5_times()
        {
            BecauseOfIssuingCommands(retryTimeInSeconds: 1);

            AssertHasCommandEntries<NewEmployeeErrorsAndRecovers>(
                _aggregateId, 
                _commandId, 
                (CommandState.Queued, 0),
                (CommandState.Started, 1),
                (CommandState.Errored, 1),
                (CommandState.Started, 2),
                (CommandState.Errored, 2),
                (CommandState.Started, 3),
                (CommandState.Errored, 3),
                (CommandState.Started, 4),
                (CommandState.Errored, 4),
                (CommandState.Started, 5),
                (CommandState.Completed, 5));
        }

        [Test]
        public void Should_have_backed_off_over_attempts()
        {
            BecauseOfIssuingCommands();

            var entries = TestContainer.GetCommandAuditEntries(_aggregateId, _commandId).OrderBy(x => x.Timestamp);
            DateTimeOffset? lastErrorTime = null;

            foreach (var commandAudit in entries)
            {
                if (commandAudit.State == CommandState.Errored)
                {
                    lastErrorTime = commandAudit.Timestamp;
                }

                if (commandAudit.State == CommandState.Started && lastErrorTime != null)
                {
                    var expectedNumberOfSecondsBetweenCalls = (commandAudit.Attempt -1) * BackOffRetryTimingStrategy.DefaultRetryBackOffInSeconds;
                    var maxNumberOfSecondsBetweenCalls = (commandAudit.Attempt - 1) * BackOffRetryTimingStrategy.DefaultRetryBackOffInSeconds + 30;
                    var timeSinceLastFail = commandAudit.Timestamp - lastErrorTime;

                    timeSinceLastFail.Value.TotalSeconds.ShouldBeGreaterThan(expectedNumberOfSecondsBetweenCalls);
                    timeSinceLastFail.Value.TotalSeconds.ShouldBeLessThanOrEqualTo(maxNumberOfSecondsBetweenCalls);
                }
            }
        }

        void BecauseOfIssuingCommands(int? retryTimeInSeconds = null)
        {
            ShopCommandHandler.Errors = 0;

            if (retryTimeInSeconds != null)
            {
                TestContainer.SetRetryTimeInSeconds(retryTimeInSeconds.Value);
            }

            _aggregateId = Guid.NewGuid();
            
            var openShop = new OpenShop(
                _aggregateId,
                "Elmore groceries",
                "Elmore",
                new DateTime(1980, 5, 20));

            TestContainer.SendCommandAndWait(openShop);

            var newEmployee = new NewEmployeeErrorsAndRecovers(_aggregateId, "Gumball", "Watson", "Shop assistant");
            _commandId = TestContainer.SendCommandAndWait(newEmployee);
        }
    }

    public class When_command_handler_raises_errors_and_abandons : CommandTestsBase
    {
        Guid _aggregateId;
        Guid _commandId;

        [Test]
        public void should_have_not_mutated_domain_with_second_command()
        {
            BecauseOfIssuingCommands(retryTimeInSeconds: 1);

            var shop = TestContainer.GetAggregate<Shop>(_aggregateId);
            shop.Id.ShouldBe(_aggregateId);
            shop.Name.ShouldBe("Elmore groceries");
            shop.City.ShouldBe("Elmore");
            shop.OpeningDate.ShouldBe(new DateTime(1980, 5, 20));
            shop.Employees.Count.ShouldBe(0);
        }

        [Test]
        public void Should_have_retried_5_times_and_abandoned()
        {
            BecauseOfIssuingCommands(retryTimeInSeconds: 1);

            AssertHasCommandEntries<NewEmployeeErrorsNeverRecovers>(
                _aggregateId,
                _commandId,
                (CommandState.Queued, 0),
                (CommandState.Started, 1),
                (CommandState.Errored, 1),
                (CommandState.Started, 2),
                (CommandState.Errored, 2),
                (CommandState.Started, 3),
                (CommandState.Errored, 3),
                (CommandState.Started, 4),
                (CommandState.Errored, 4),
                (CommandState.Started, 5),
                (CommandState.Errored, 5),
                (CommandState.Abandoned, 5));
        }

        void BecauseOfIssuingCommands(int? retryTimeInSeconds = null)
        {
            ShopCommandHandler.Errors = 0;

            if (retryTimeInSeconds != null)
            {
                TestContainer.SetRetryTimeInSeconds(retryTimeInSeconds.Value);
            }

            _aggregateId = Guid.NewGuid();

            var openShop = new OpenShop(
                _aggregateId,
                "Elmore groceries",
                "Elmore",
                new DateTime(1980, 5, 20));

            TestContainer.SendCommandAndWait(openShop);

            var newEmployee = new NewEmployeeErrorsNeverRecovers(_aggregateId, "Gumball", "Watson", "Shop assistant");
            _commandId = TestContainer.SendCommandAndWait(newEmployee);
        }
    }

}
