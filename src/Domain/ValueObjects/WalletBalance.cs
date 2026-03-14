using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record WalletBalance : ValueObject
    {
        public Money Available { get; }
        public Money Pending { get; }

        private WalletBalance(Money available, Money pending)
        {
            Available = available;
            Pending = pending;
        }

        public static WalletBalance Create(Money available, Money pending)
        {
            return new WalletBalance(available, pending);
        }

        public WalletBalance AddPending(Money amount) =>
            new(Available, Pending.Add(amount));

        public WalletBalance MovePendingToAvailable(Money amount)
        {
            if (amount >= Pending)
            {
                throw new BusinessRuleValidationException("WalletBalance.PendingTooLow",
                    "Le montant pending est insuffisant.");
            }

            return new WalletBalance(Available.Add(amount), Money.From(Pending.Amount - amount.Amount, Pending.Currency));
        }

        public WalletBalance Withdraw(Money amount)
        {
            if (amount >= Available)
            {
                throw new BusinessRuleValidationException("WalletBalance.NotEnoughFunds",
                    "Le solde disponible est insuffisant pour ce retrait.");
            }

            return new WalletBalance(
                Money.From(Available.Amount - amount.Amount, Available.Currency),
                Pending);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Available;
            yield return Pending;
        }
    }
}

