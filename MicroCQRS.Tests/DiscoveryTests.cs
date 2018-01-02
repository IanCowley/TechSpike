using System;
using MicroCQRS.Tests.TestDomain.CommandHandlers;
using MicroCQRS.Tests.TestDomain.Commands;
using MicroCQRS.Tests.TestDomain.Model;
using NUnit.Framework;
using Shouldly;

namespace MicroCQRS.Tests
{
    public class DiscoveryTests
    {
        [TestFixture]
        public class When_specifying_custom_activator
        {
            TestContext _testContext;
            Guid _shopId;

            [SetUp]
            public void BecauseOf()
            {
                _shopId = Guid.NewGuid();

                _testContext = TestContainer.NewContext(type =>
                {
                    if (type == typeof(ShopCommandHandler))
                    {
                        return new ShopCommandHandler(new DateTime(1980, 5, 20));
                    }

                    return (ICommandHandler)Activator.CreateInstance(type);
                });

                _testContext.StartProcessing();

                _testContext.SendCommandAndWaitAsync(new OpenShop(
                    shopId: _shopId,
                    name: "Mos Eisley Cantina",
                    city: "Most Eisley",
                    openingDate: new DateTime(1977, 5, 25))).Wait();
            }

            [Test]
            public void It_should_have_used_custom_constructor()
            {
                var shop = _testContext.GetAggregateAsync<Shop>(_shopId).Result;
                shop.OpeningDate.ShouldBe(new DateTime(1980, 5, 20));
            }

            [TearDown]
            public void Cleanup()
            {
                _testContext.CleanUpAsync().Wait();
            }
        }

        [TestFixture]
        public class When_not_specifying_custom_activator
        {
            Guid _shopId;

            [SetUp]
            public void BecauseOf()
            {
                _shopId = Guid.NewGuid();

                TestContainer.SendCommandAndWait(new OpenShop(
                    shopId: _shopId,
                    name: "Mos Eisley Cantina",
                    city: "Most Eisley",
                    openingDate: new DateTime(1977, 5, 25)));
            }

            [Test]
            public void It_should_have_used_custom_constructor()
            {
                var shop = TestContainer.GetAggregate<Shop>(_shopId);
                shop.OpeningDate.ShouldBe(new DateTime(1977, 5, 25));
            }
        }
    }
}
