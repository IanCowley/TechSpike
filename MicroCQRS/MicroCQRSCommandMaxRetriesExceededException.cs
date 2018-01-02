using System;

namespace MicroCQRS
{
    public class MicroCQRSCommandMaxRetriesExceededException : Exception
    {
        public MicroCQRSCommandMaxRetriesExceededException(int maxRetries, Exception exception) : base($"Max retries exceeded, tried {maxRetries} times", exception)
        {
            
        }
    }
}