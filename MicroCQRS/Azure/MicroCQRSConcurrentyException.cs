using System;
using Microsoft.WindowsAzure.Storage;

namespace MicroCQRS.Azure
{
    public class MicroCQRSConcurrentyException : Exception
    {
        public MicroCQRSConcurrentyException(StorageException sex) : base("Concurrency exception", sex)
        {
        }
    }
}