using System.Collections.Generic;

namespace MicroCQRS.Tests.TestDomain.Model
{
    public class Sales : Aggregate
    {
        public Sales()
        {
            RunningTotal = 0;
            _Sales = new List<double>();
        }

        public List<double> _Sales { get; set; }

        public double RunningTotal { get; set; }

        public void AddSale(double value)
        {
            RunningTotal += value;
            _Sales.Add(value);
        }
    }
}
