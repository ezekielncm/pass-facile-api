using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record Capacity : ValueObject
    {
        public int Total { get; }
        public int UsedCount { get; }

        private Capacity(int total, int usedCount)
        {
            Total = total;
            UsedCount = usedCount;
        }
        public Capacity() { }

        public static Capacity From(int total, int usedCount = 0)
        {
            if (total < 0)
            {
                throw new BusinessRuleValidationException("Capacity.Negative",
                    "La capacité totale ne peut pas être négative.");
            }

            if (usedCount < 0)
            {
                throw new BusinessRuleValidationException("Capacity.NegativeUsed",
                    "Le nombre utilisé ne peut pas être négatif.");
            }

            return new Capacity(total, usedCount);
        }

        public Capacity Add(int value) => From(checked(Total + value), UsedCount);

        public bool IsFull() => UsedCount >= Total;

        public bool IsNearFull(decimal threshold) =>
            Total > 0 && (decimal)UsedCount / Total >= threshold;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Total;
            yield return UsedCount;
        }

        public override string ToString() => $"{UsedCount}/{Total}";
    }
}
