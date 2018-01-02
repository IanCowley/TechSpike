using System;

namespace MicroCQRS
{
    public class Aggregate
    {
        public Guid Id { get; set; }

        public string Version { get; set; }

        public Aggregate()
        {
            Id = Guid.NewGuid();
        }
    }
}
