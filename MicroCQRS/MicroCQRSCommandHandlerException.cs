using System;

namespace MicroCQRS
{
    public class MicroCQRSCommandHandlerException : Exception
    {
        public Type CommandType { get; }
        public Guid CommandAggregateId { get; }
        public Exception Exception { get; }
        public int AttemptNumber { get; }

        public MicroCQRSCommandHandlerException(
            Type commandType, 
            Guid commandAggregateId, 
            Exception exception, 
            int attemptNumber) : 
            base($"Command {commandType} experienced an exception while processing for aggregate {commandAggregateId}", exception)
        {
            CommandType = commandType;
            CommandAggregateId = commandAggregateId;
            Exception = exception;
            AttemptNumber = attemptNumber;
        }
    }
}