using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCQRS
{
    public interface ICommandProcessingScheduler
    {
        Task Start(int maxParallalisation, Func<Task> process, CancellationToken cancellationToken);
        void Stop();
    }
}
