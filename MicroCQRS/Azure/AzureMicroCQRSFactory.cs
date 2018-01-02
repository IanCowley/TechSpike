using System;
using MicroCQRS.Azure;

namespace MicroCQRS
{
    public static class AzureMicroCQRSFactory
    {
        public static MicroCQRS Build(
            string storageAccountName,
            string storageAccountKey,
            string storagePrefix = null,
            Func<Type, ICommandHandler> activator = null)
        {
            return MicroCQRS.Build(
                new AzureRepositoryProvider(storageAccountName, storageAccountKey, storagePrefix),
                activator);
        }
    }
}
