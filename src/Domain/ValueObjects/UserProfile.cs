using Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects
{
    public sealed record UserProfile(
        string DisplayName,
        string Bio,
        string LogoUrl,
        string BannerUrl,
        string Slug
        ):ValueObject
    {
        //public string FullName => $"{FirstName} {LastName}";
        //public int Age => DateTime.Today.Year - BirthDate.Year - (BirthDate.Date > DateTime.Today.AddYears(-Age) ? 1 : 0);
        //public bool IsAdult => Age >= 18;
        public UserProfile WithDisplayName(string displayName) => this with { DisplayName = displayName };
        //public UserProfile WithLastName(string lastName) => this with { LastName = lastName };
        //public UserProfile() { }
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return DisplayName;
            yield return Bio;
            yield return LogoUrl;
            yield return BannerUrl;
            yield return Slug;
        }
    }
}
