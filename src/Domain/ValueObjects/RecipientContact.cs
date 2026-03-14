using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record RecipientContact : ValueObject
    {
        public string Value { get; }

        private RecipientContact(string value)
        {
            Value = value;
        }

        public static RecipientContact From(string value)
        {
            Guard.Against.NullOrEmpty(value, nameof(value));
            return new RecipientContact(value.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}

