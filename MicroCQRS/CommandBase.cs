using System;

namespace MicroCQRS
{
    public abstract class CommandBase : ICommand
    {
        public Guid Id { get; set; }
        public Guid AggregateId { get; set; }

        protected CommandBase(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
