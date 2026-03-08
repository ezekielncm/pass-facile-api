using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects
{
    public sealed record UserProfile(
        string FirstName,
        string LastName,
        DateTime BirthDate)
    {
        public string FullName => $"{FirstName} {LastName}";
        public int Age => DateTime.Today.Year - BirthDate.Year - (BirthDate.Date > DateTime.Today.AddYears(-Age) ? 1 : 0);
        public bool IsAdult => Age >= 18;
        public UserProfile WithFirstName(string firstName) => this with { FirstName = firstName };
        public UserProfile WithLastName(string lastName) => this with { LastName = lastName };
    }
}
