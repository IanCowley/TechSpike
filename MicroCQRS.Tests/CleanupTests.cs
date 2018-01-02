using NUnit.Framework;

namespace MicroCQRS.Tests
{
    [TestFixture]
    public class CleanupTests
    {
        [Test]
        public void Cleaning()
        {
            TestContainer.CleanUpAll();
        }
    }
}