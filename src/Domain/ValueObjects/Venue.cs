using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record Venue : ValueObject
    {
        public string Name { get; }
        public string AddressLine1 { get; }
        public string? AddressLine2 { get; }
        public string City { get; }
        public string Country { get; }

        private Venue(
            string name,
            string addressLine1,
            string? addressLine2,
            string city,
            string country)
        {
            Name = name;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            City = city;
            Country = country;
        }

        public static Venue Create(
            string name,
            string addressLine1,
            string? addressLine2,
            string city,
            string country)
        {
            Guard.Against.NullOrEmpty(name, nameof(name));
            Guard.Against.NullOrEmpty(addressLine1, nameof(addressLine1));
            Guard.Against.NullOrEmpty(city, nameof(city));
            Guard.Against.NullOrEmpty(country, nameof(country));

            return new Venue(
                name.Trim(),
                addressLine1.Trim(),
                string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim(),
                city.Trim(),
                country.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
            yield return AddressLine1;
            yield return AddressLine2;
            yield return City;
            yield return Country;
        }

        public override string ToString() => $"{Name}, {AddressLine1}, {City}, {Country}";
    }
}
