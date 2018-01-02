using System;

namespace MicroCQRS.Tests.TestDomain.Commands
{
    public class MadeSale : CommandBase
    {
        public double Value { get; set; }

        public MadeSale(Guid shopId, double value) : base(shopId)
        {
            Value = value;
        }
    }
}
