using System;

namespace MicroCQRS
{
    public interface IRetryTimingStrategy
    {
        int GetNumberOfSecondsToWaitForNextAttempt(int attemptNumber);
    }

    public class BackOffRetryTimingStrategy : IRetryTimingStrategy
    {
        readonly Random _retryRandom;
        public const int DefaultRetryBackOffInSeconds = 30;
        public int RetryBackOffInSeconds { get; set; }
        
        public BackOffRetryTimingStrategy()
        {
            RetryBackOffInSeconds = DefaultRetryBackOffInSeconds;
            _retryRandom = new Random();
        }

        public int GetNumberOfSecondsToWaitForNextAttempt(int attemptNumber)
        {
            return attemptNumber * RetryBackOffInSeconds + _retryRandom.Next(0, RetryBackOffInSeconds);
        }
    }
}
