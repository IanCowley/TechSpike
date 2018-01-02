using System;

namespace MicroCQRS.Tests.TestDomain.Commands
{
    public class YouCantHandleTheCommand : CommandBase
    {
        public YouCantHandleTheCommand(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}
