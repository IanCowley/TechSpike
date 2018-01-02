using System;

namespace MicroCQRS
{
    public interface ICommand
    {
        Guid Id { get; set; }
        Guid AggregateId { get; }
    }
}