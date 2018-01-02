using System.Collections.Generic;

namespace TechSpike.Domain
{
    public class BatchAccounts
    {
        public Dictionary<string, BatchAccount> Accounts;

        public BatchAccounts()
        {
            Accounts = new Dictionary<string, BatchAccount>();
        }

        public void TrackAccount(string accountName, string accountKey, string url)
        {
            Accounts.Add(accountName, new BatchAccount
            {
                AccountName = accountName,
                
            });
        }
    }

    public class BatchAccount
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string URL { get; set; }
    }
}
