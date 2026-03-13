using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class PaymentMethod : ValueObject
    {
        public string Value { get; }

        private PaymentMethod(string value)
        {
            Value = value;
        }

        public static PaymentMethod From(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));
            return new PaymentMethod(value.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
