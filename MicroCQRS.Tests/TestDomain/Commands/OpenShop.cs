using System;

namespace MicroCQRS.Tests.TestDomain.Commands
{
    public class OpenShop : CommandBase
    {
        public string Name { get; set; }
        public string City { get; set; }
        public DateTime? OpeningDate { get; set; }

        public OpenShop(Guid shopId, string name, string city, DateTime? openingDate = null) : base(shopId)
        {
            Name = name;
            City = city;
            OpeningDate = openingDate;
        }
    }
}
