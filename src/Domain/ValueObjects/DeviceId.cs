using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class DeviceId : ValueObject
    {
        public string Value { get; }

        private DeviceId(string value)
        {
            Value = value;
        }

        public static DeviceId From(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));
            return new DeviceId(value.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}

