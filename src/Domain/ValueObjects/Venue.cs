using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record Venue : ValueObject
    {
        public string Name { get; }
        public string City { get; }
        public string Address { get; }
        public string? GpsCoordinates { get; }

        private Venue(
            string name,
            string city,
            string address,
            string? gpsCoordinates)
        {
            Name = name;
            City = city;
            Address = address;
            GpsCoordinates = gpsCoordinates;
        }
        public Venue() { }
        public static Venue Create(
            string name,
            string city,
            string address,
            string? gpsCoordinates = null)
        {
            Guard.Against.NullOrEmpty(name, nameof(name));
            Guard.Against.NullOrEmpty(city, nameof(city));
            Guard.Against.NullOrEmpty(address, nameof(address));

            return new Venue(
                name.Trim(),
                city.Trim(),
                address.Trim(),
                string.IsNullOrWhiteSpace(gpsCoordinates) ? null : gpsCoordinates.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
            yield return City;
            yield return Address;
            yield return GpsCoordinates;
        }

        public override string ToString() => $"{Name}, {Address}, {City}";
    }
}
