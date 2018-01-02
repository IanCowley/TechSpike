using System;

namespace MicroCQRS
{
    public class MicroCQRSException : Exception
    {
        public MicroCQRSException(string message) : base(message)
        {

        }

        public MicroCQRSException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}