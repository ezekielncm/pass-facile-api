using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class TransactionId : ValueObject
    {
        public string Value { get; }

        private TransactionId(string value)
        {
            Value = value;
        }

        public static TransactionId From(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));
            return new TransactionId(value.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
