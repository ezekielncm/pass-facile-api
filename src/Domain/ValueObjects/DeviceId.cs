using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record DeviceId : ValueObject
    {
        public string Value { get; }

        private DeviceId(string value)
        {
            Value = value;
        }
        public DeviceId() { }
        public static DeviceId From(string value)
        {
            Guard.Against.NullOrEmpty(value, nameof(value));
            return new DeviceId(value.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}

