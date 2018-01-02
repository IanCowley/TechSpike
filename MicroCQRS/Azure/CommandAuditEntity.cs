using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MicroCQRS.Azure
{
    public class CommandAuditEntity : TableEntity
    {
        public CommandAuditEntity(Guid aggregateId, Guid id) : base(aggregateId.ToString(), id.ToString())
        {
        }

        public CommandAuditEntity()
        {
            
        }

        public string Content { get; set; }
        public int State { get; set; }
        public int Attempt { get; set; }
        public string Type { get; set; }
        public string CommandId { get; set; }
    }
}
