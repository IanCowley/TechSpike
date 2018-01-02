namespace MicroCQRS.Azure
{
    public class CommandAttempt
    {
        public ICommand Command { get; set; }
        public int Attempt { get; set; }
        public CommandState CommandState { get; set; }
        public string PopReceipt { get; set; }
        public string MessageId { get; set; }
    }
}