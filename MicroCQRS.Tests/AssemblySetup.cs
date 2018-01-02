using NUnit.Framework;

[SetUpFixture]
public class AssemblySetup
{
    [OneTimeSetUp]
    public void Setup()
    {
        MicroCQRS.Tests.TestContainer.Initialise();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        MicroCQRS.Tests.TestContainer.CleanUp();
    }
}