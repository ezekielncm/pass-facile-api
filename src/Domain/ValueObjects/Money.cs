using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money From(decimal amount, string currency = "XOF")
        {
            if (amount < 0)
            {
                throw new BusinessRuleValidationException("Money.Negative", "Le montant ne peut pas être négatif.");
            }

            Guard.Against.NullOrEmpty(currency, nameof(currency));

            return new Money(decimal.Round(amount, 2), currency.ToUpperInvariant());
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
            {
                throw new BusinessRuleValidationException("Money.CurrencyMismatch", "Les devises doivent être identiques.");
            }

            return From(Amount + other.Amount, Currency);
        }

        public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount && left.Currency == right.Currency;
        public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount && left.Currency == right.Currency;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
