namespace MicroCQRS.Tests.Utilities
{
    public class RetryImmediately : IRetryTimingStrategy
    {
        public int GetNumberOfSecondsToWaitForNextAttempt(int attemptNumber)
        {
            return 0;
        }
    }
}
