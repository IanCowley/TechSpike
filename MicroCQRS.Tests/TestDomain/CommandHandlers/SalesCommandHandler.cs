using System;
using System.Threading.Tasks;
using MicroCQRS.Tests.TestDomain.Commands;
using MicroCQRS.Tests.TestDomain.Model;

namespace MicroCQRS.Tests.TestDomain.CommandHandlers
{
    public class SalesCommandHandler : CommandHandlerBase<Sales>, IHandleCommand<MadeSale>
    {
        public static Action SalesCallBack { get; set; }

        public async Task HandlesAsync(MadeSale command)
        {
            await PerformUnitOfWorkAsync(command, sales =>
            {
                SalesCallBack?.Invoke();
                sales.AddSale(command.Value);
            });
        }
    }
}
