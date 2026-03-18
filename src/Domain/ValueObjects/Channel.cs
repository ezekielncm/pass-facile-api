using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record Channel : ValueObject
    {
        public string Value { get; }

        private Channel(string value)
        {
            Value = value;
        }
        public Channel() { }
        public static Channel From(string value)
        {
            Guard.Against.NullOrEmpty(value, nameof(value));
            return new Channel(value.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}

