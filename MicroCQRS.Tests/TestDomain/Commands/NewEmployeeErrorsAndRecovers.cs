using System;

namespace MicroCQRS.Tests.TestDomain.Commands
{
    public class NewEmployeeErrorsAndRecovers : CommandBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }

        public NewEmployeeErrorsAndRecovers(Guid employeeId, string firstName, string lastName, string position) : base(employeeId)
        {
            FirstName = firstName;
            LastName = lastName;
            Position = position;
        }
    }
}
