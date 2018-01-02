using System;

namespace MicroCQRS
{
    public class MicroCQRSConfigurationException : Exception
    {
        public MicroCQRSConfigurationException(string message) : base(message)
        {
            
        }
    }
}