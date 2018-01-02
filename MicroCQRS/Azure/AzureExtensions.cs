using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MicroCQRS.Azure
{
    public static class AzureExtensions
    {
        public static async Task ExecuteAsyncIgnore409(this CloudTable table, TableOperation tableOperation)
        {
            try
            {
                await table.ExecuteAsync(tableOperation);
            }
            catch (StorageException storageException)
            {
                if (storageException.RequestInformation.HttpStatusCode != (int) HttpStatusCode.Conflict)
                {
                    throw;
                }
            }
        }
    }
}
