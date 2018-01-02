using System;
using System.Linq;
using System.Threading;
using MicroCQRS.Azure;
using MicroCQRS.Tests.TestDomain.CommandHandlers;
using MicroCQRS.Tests.TestDomain.Commands;
using MicroCQRS.Tests.Utilities;
using NUnit.Framework;
using Shouldly;

namespace MicroCQRS.Tests
{
    public class MicroCQRSTests
    {
        [TestFixture]
        public class when_server_already_running
        {
            TestContext _context;

            [SetUp]
            public void BecauseOf_starting_server()
            {
                _context = TestContainer.NewContext();
                _context.StartProcessing();
            }

            [Test]
            public void It_should_not_start_new_threads()
            {
                ThreadPool.GetAvailableThreads(out var initialWorkingThreads, out var initialCompletionPortThreads);

                _context.StartProcessing();

                ThreadPool.GetAvailableThreads(out var finalWorkingThreads, out var finalCompletionPortThreads);

                _context.StopProcessing();

                Assert.AreEqual(initialWorkingThreads, finalWorkingThreads);

            }
        }

        [TestFixture]
        public class when_specifying_alternate_retry : CommandTestsBase
        {
            TestContext _context;
            Guid _aggregateId;
            Guid _commandId;

            [SetUp]
            public void BecauseOf_starting_server()
            {
                _context = TestContainer.NewContext();
                _context.StartProcessing(new RetryImmediately());
                BecauseOfIssuingCommands();
            }

            [Test]
            public void Should_have_immediately_retried()
            {
                BecauseOfIssuingCommands();

                var entries = _context.GetCommandAuditEntriesAsync(_aggregateId, _commandId).Result.OrderBy(x => x.Timestamp);
                DateTimeOffset? lastErrorTime = null;

                foreach (var commandAudit in entries)
                {
                    if (commandAudit.State == CommandState.Errored)
                    {
                        lastErrorTime = commandAudit.Timestamp;
                    }

                    if (commandAudit.State == CommandState.Started && lastErrorTime != null)
                    {
                        var expectedNumberOfSecondsBetweenCalls = 2;
                        var timeSinceLastFail = commandAudit.Timestamp - lastErrorTime;

                        timeSinceLastFail.Value.TotalSeconds.ShouldBeLessThan(expectedNumberOfSecondsBetweenCalls);
                    }
                }
            }

            void BecauseOfIssuingCommands()
            {
                ShopCommandHandler.Errors = 0;

                _aggregateId = Guid.NewGuid();
                var openShop = new OpenShop(_aggregateId, "Elmore groceries", "Elmore", new DateTime(1980, 5, 20));
                _context.SendCommandAndWaitAsync(openShop).Wait();

                var newEmployee = new NewEmployeeErrorsAndRecovers(_aggregateId, "Gumball", "Watson", "Shop assistant");
                _commandId = _context.SendCommandAndWaitAsync(newEmployee).Result;
            }
        }

        public class when_specifying_polling_interval
        {

        }

    }
}
