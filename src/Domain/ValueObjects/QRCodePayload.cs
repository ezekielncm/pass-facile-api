using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class QRCodePayload : ValueObject
    {
        public string Value { get; }

        private QRCodePayload(string value)
        {
            Value = value;
        }

        public static QRCodePayload From(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));
            return new QRCodePayload(value.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}

