using Domain.Common;

namespace Domain.ValueObjects.Identities
{
    public sealed record OrderId : ValueObject
    {
        public Guid Value { get; }

        private OrderId(Guid value)
        {
            Value = value;
        }

        public static OrderId NewId() => new(Guid.NewGuid());
        public static OrderId From(Guid value) => new(value);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
    }
}
