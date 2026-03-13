using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class TicketReference : ValueObject
    {
        public string Value { get; }

        private TicketReference(string value)
        {
            Value = value;
        }

        public static TicketReference From(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));
            return new TicketReference(value.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}

