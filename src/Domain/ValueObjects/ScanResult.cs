using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class ScanResult : ValueObject
    {
        public string Value { get; }

        private ScanResult(string value)
        {
            Value = value;
        }

        public static ScanResult From(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));
            return new ScanResult(value.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}

