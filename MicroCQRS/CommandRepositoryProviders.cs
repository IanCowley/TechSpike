using MicroCQRS.Azure;

namespace MicroCQRS
{
    public class CommandRepositoryProviders
    {
        public static IRepositoryProvider Azure(string storageAccountName, string storageAccountKey, string prefix = null)
        {
            return new AzureRepositoryProvider(storageAccountName, storageAccountKey, prefix);
        }
    }
}