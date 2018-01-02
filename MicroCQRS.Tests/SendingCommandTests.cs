using System;
using System.Linq;
using MicroCQRS.Azure;
using MicroCQRS.Tests.TestDomain.Commands;
using MicroCQRS.Tests.TestDomain.Model;
using NUnit.Framework;
using Shouldly;

namespace MicroCQRS.Tests
{
    public class Sending_commands : CommandTestsBase
    {
        protected Guid _aggregateId;
        protected Guid _commandId;
    }

    [TestFixture]
    public class When_sending_a_command : Sending_commands
    {
        [SetUp]
        public void BecauseOf_sending_command()
        {
            _aggregateId = Guid.NewGuid();

            var command = new OpenShop(_aggregateId, "Elmore groceries", "Elmore", new DateTime(1980, 5, 20));
            _commandId = TestContainer.SendCommandAndWait(command);
        }

        [Test]
        public void It_should_have_mutated_aggregate()
        {
            var shop = TestContainer.GetAggregate<Shop>(_aggregateId);
            shop.Id.ShouldBe(_aggregateId);
            shop.Name.ShouldBe("Elmore groceries");
            shop.City.ShouldBe("Elmore");
            shop.OpeningDate.ShouldBe(new DateTime(1980, 5, 20));
        }

        [Test]
        public void It_should_have_created_command_audit_entries()
        {
            AssertHasCommandEntries<OpenShop>(
                _aggregateId,
                _commandId,
                (CommandState.Queued, 0),
                (CommandState.Started, 1),
                (CommandState.Completed, 1));
        }
    }

    [TestFixture]
    public class when_issuing_a_command_command_id : Sending_commands
    {
        [SetUp]
        public void BecauseOf_sending_command_with_command_id()
        {
            _aggregateId = Guid.NewGuid();
            _commandId = Guid.NewGuid();
            var command = new OpenShop(_aggregateId, "Elmore groceries", "Elmore", new DateTime(1980, 5, 20));
            _commandId = TestContainer.SendCommandAndWait(command);
        }

        [Test]
        public void It_should_have_created_command_audit_entries()
        {
            AssertHasCommandEntries<OpenShop>(
                _aggregateId,
                _commandId,
                (CommandState.Queued, 0),
                (CommandState.Started, 1),
                (CommandState.Completed, 1));
        }
    }

    [TestFixture]
    public class when_mutating_aggregate_multiple_times : Sending_commands
    {
        Guid _firstCommandId;
        Guid _secondCommandId;

        [SetUp]
        public void BecauseOf_sending_command_with_command_id()
        {
            _aggregateId = Guid.NewGuid();
            _firstCommandId = Guid.NewGuid();
            _secondCommandId = Guid.NewGuid();

            _firstCommandId = TestContainer.SendCommandAndWait(new OpenShop(_aggregateId, "Elmore groceries", "Elmore", new DateTime(1980, 5, 20)));
            _secondCommandId = TestContainer.SendCommandAndWait(new NewEmployee(_aggregateId, "Gumball", "Watson", "Shop assistant"));
        }

        [Test]
        public void It_should_have_mutated_aggregate_twice()
        {
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
        public void Should_have_retried_command_entries()
        {
            AssertHasCommandEntries<OpenShop>(
                _aggregateId,
                _firstCommandId,
                (CommandState.Queued, 0),
                (CommandState.Started, 1),
                (CommandState.Completed, 1));

            AssertHasCommandEntries<NewEmployee>(
                _aggregateId,
                _secondCommandId,
                (CommandState.Queued, 0),
                (CommandState.Started, 1),
                (CommandState.Completed, 1));
        }
    }

    [TestFixture]
    public class when_no_command_handler : Sending_commands
    {
        [SetUp]
        public void BecauseOf_sending_command()
        {
            _aggregateId = Guid.NewGuid();
            _commandId = TestContainer.SendCommandAndWait(new YouCantHandleTheCommand(_aggregateId));
        }

        [Test]
        public void It_should_have_logged_command_not_found()
        {
            AssertHasCommandEntries<YouCantHandleTheCommand>(
                _aggregateId,
                _commandId,
                (CommandState.Queued, 0),
                (CommandState.HandlerNotFound, 1));
        }
    }
}

// Polly retry stuffs - CommandExecution needs it, so does the command repository
// Load tests
// when one thread fails while it's waiting to retry it should be processing other stuffs
// IDEMPOTENCY TESTS > If I have the wrong state, number of attempts or crashing during saves etc...
// add sale, good 409 scenario!!! concurrency ting, idempotency - what if the stock ain't there!
