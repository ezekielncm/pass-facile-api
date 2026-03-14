using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record Capacity : ValueObject
    {
        public int Total { get; }

        private Capacity(int total)
        {
            Total = total;
        }

        public static Capacity From(int total)
        {
            if (total < 0)
            {
                throw new BusinessRuleValidationException("Capacity.Negative",
                    "La capacité totale ne peut pas être négative.");
            }

            return new Capacity(total);
        }

        public Capacity Add(int value) => From(checked(Total + value));

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Total;
        }

        public override string ToString() => Total.ToString();
    }
}
