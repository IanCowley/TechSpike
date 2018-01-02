using System;
using System.Threading.Tasks;
using MicroCQRS.Azure;
using MicroCQRS.Tests.TestDomain.Commands;
using MicroCQRS.Tests.TestDomain.Model;

namespace MicroCQRS.Tests.TestDomain.CommandHandlers
{
    public class ShopCommandHandler : CommandHandlerBase<Shop>, 
        IHandleCommand<OpenShop>,
        IHandleCommand<NewEmployeeErrorsAndRecovers>,
        IHandleCommand<NewEmployee>,
        IHandleCommand<NewEmployeeErrorsNeverRecovers>
    {
        public DateTime? SeedSeedOpeningDate { get; set; }

        public static int Errors { get; set; }

        public ShopCommandHandler()
        {
        }

        public ShopCommandHandler(DateTime seedOpeningDate)
        {
            SeedSeedOpeningDate = seedOpeningDate;
        }

        public async Task HandlesAsync(OpenShop command)
        {
            await PerformUnitOfWorkAsync(command, shop =>
            {
                var openingDate = command.OpeningDate;

                if (SeedSeedOpeningDate != null)
                {
                    openingDate = SeedSeedOpeningDate;
                }

                shop.Open(command.Name, command.City, openingDate ?? DateTime.Now);
            });
        }

        public async Task HandlesAsync(NewEmployeeErrorsAndRecovers command)
        {
            await PerformUnitOfWorkAsync(command, shop =>
            {
                if (Errors < 4)
                {
                    Errors++;
                    throw new Exception("Testing error handling");
                }

                shop.NewEmployee(command.FirstName, command.LastName, command.Position);
            });
        }

        public async Task HandlesAsync(NewEmployeeErrorsNeverRecovers command)
        {
            await PerformUnitOfWorkAsync(command, shop => throw new Exception("Testing error handling"));
        }

        public async Task HandlesAsync(NewEmployee command)
        {
            await PerformUnitOfWorkAsync(command, shop =>
            {
                shop.NewEmployee(command.FirstName, command.LastName, command.Position);
            });
        }
    }
}
