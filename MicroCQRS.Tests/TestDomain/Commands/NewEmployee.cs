using System;

namespace MicroCQRS.Tests.TestDomain.Commands
{
    public class NewEmployee : CommandBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }

        public NewEmployee(Guid employeeId, string firstName, string lastName, string position) : base(employeeId)
        {
            FirstName = firstName;
            LastName = lastName;
            Position = position;
        }
    }
}
