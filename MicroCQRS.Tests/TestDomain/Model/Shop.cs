using System;
using System.Collections.Generic;

namespace MicroCQRS.Tests.TestDomain.Model
{
    public class Shop : Aggregate
    {
        public Shop()
        {
            Employees = new List<Employee>();
        }

        public string Name { get; set; }
        public string City { get; set; }
        public DateTime OpeningDate { get; set; }
        public List<Employee> Employees { get; set; }

        public void Open(string name, string city, DateTime openingDate)
        {
            Name = name;
            City = city;
            OpeningDate = openingDate;
        }

        public void NewEmployee(string firstName, string lastName, string position)
        {
            if(!Employees.Exists(x => 
                x.FirstName == firstName &
                x.LastName == lastName &
                x.Position == position
            ))
            {
                Employees.Add(new Employee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Position = position
                });
            }
        }
    }

    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
    }
}
