using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCQRS
{
    public class SpinLoopScheduler : ICommandProcessingScheduler
    {
        TimeSpan _loopDelay;

        public SpinLoopScheduler() : this(TimeSpan.FromMilliseconds(600))
        {
        }
        
        public SpinLoopScheduler(TimeSpan loopDelay)
        {
            _loopDelay = loopDelay;
        }

        public async Task Start(int maxParallalisation, Func<Task> process, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            for (int processorIndex = 0; processorIndex < maxParallalisation; processorIndex++)
            {
                tasks.Add(Task.Run(() => Poll(process, cancellationToken), cancellationToken));
            }

            await Task.WhenAll(tasks.ToArray());
        }

        async Task Poll(Func<Task> process, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await process();
                await Task.Delay(_loopDelay, cancellationToken);
            }
        }

        public void Stop()
        {
            
        }
    }
}