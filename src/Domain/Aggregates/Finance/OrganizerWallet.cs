using Domain.Common;
using Domain.DomainEvents.Finance;
using Domain.ValueObjects;

namespace Domain.Aggregates.Finance
{
    public sealed class OrganizerWallet : AggregateRoot<Guid>
    {
        private readonly List<WithdrawalRequest> _withdrawals = [];
        private readonly List<FeeTransaction> _fees = [];

        public Guid OrganizerId { get; private set; }
        public WalletBalance Balance { get; private set; } = WalletBalance.Create(Money.From(0), Money.From(0));

        public IReadOnlyCollection<WithdrawalRequest> Withdrawals => _withdrawals.AsReadOnly();
        public IReadOnlyCollection<FeeTransaction> Fees => _fees.AsReadOnly();

        // EF
        private OrganizerWallet() { }

        private OrganizerWallet(Guid id, Guid organizerId)
            : base(id)
        {
            OrganizerId = organizerId;
        }

        public static OrganizerWallet Create(Guid organizerId)
        {
            return new OrganizerWallet(Guid.NewGuid(), organizerId);
        }

        public void ReceiveRevenue(Money amount, Money platformFee, DateTimeOffset eventEndDate, DateTimeOffset now)
        {
            // Les frais de plateforme sont déduits avant crédit du wallet
            var net = Money.From(amount.Amount - platformFee.Amount, amount.Currency);

            _fees.Add(FeeTransaction.Create(Id, platformFee));

            // Le solde devient disponible 24h après la fin de l'événement
            if (now >= eventEndDate.AddHours(24))
            {
                Balance = WalletBalance.Create(
                    Balance.Available.Add(net),
                    Balance.Pending);
            }
            else
            {
                Balance = Balance.AddPending(net);
            }

            RaiseEvent(new RevenueReceived(Id, net));
        }

        public void MakePendingAvailable(DateTimeOffset eventEndDate, DateTimeOffset now)
        {
            if (now < eventEndDate.AddHours(24))
            {
                return;
            }

            var pending = Balance.Pending;
            if (pending.Amount <= 0) return;

            Balance = Balance.MovePendingToAvailable(pending);
        }

        public WithdrawalRequest RequestWithdrawal(Money amount)
        {
            // Un retrait ne peut excéder le solde disponible
            Balance = Balance.Withdraw(amount);

            var withdrawal = WithdrawalRequest.Create(Id, amount);
            _withdrawals.Add(withdrawal);

            RaiseEvent(new WithdrawalRequested(Id, amount));
            return withdrawal;
        }

        public void MarkWithdrawalProcessed(Guid withdrawalId)
        {
            var withdrawal = _withdrawals.FirstOrDefault(withdrawalRequest => withdrawalRequest.Id == withdrawalId);
            if (withdrawal is null) return;

            withdrawal.MarkProcessed();
            RaiseEvent(new WithdrawalProcessed(Id, withdrawal.Id));
        }

        public void MarkWithdrawalFailed(Guid withdrawalId)
        {
            var withdrawal = _withdrawals.FirstOrDefault(withdrawalRequest => withdrawalRequest.Id == withdrawalId);

            if (withdrawal is null) return;

            withdrawal.MarkFailed();
            // On pourrait re-créditer `Balance.Available` ici si nécessaire
            RaiseEvent(new WithdrawalFailed(Id, withdrawal.Id));
        }
    }

    public sealed class WithdrawalRequest : Entity<Guid>
    {
        public Guid OrganizerWalletId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public WithdrawalStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        // EF
        private WithdrawalRequest() { }

        private WithdrawalRequest(Guid id, Guid walletId, Money amount)
            : base(id)
        {
            OrganizerWalletId = walletId;
            Amount = amount;
            Status = WithdrawalStatus.Pending;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static WithdrawalRequest Create(Guid walletId, Money amount)
        {
            return new WithdrawalRequest(Guid.NewGuid(), walletId, amount);
        }

        public void MarkProcessed()
        {
            Status = WithdrawalStatus.Processed;
        }

        public void MarkFailed()
        {
            Status = WithdrawalStatus.Failed;
        }
    }

    public sealed class FeeTransaction : Entity<Guid>
    {
        public Guid OrganizerWalletId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }

        // EF
        private FeeTransaction() { }

        private FeeTransaction(Guid id, Guid walletId, Money amount)
            : base(id)
        {
            OrganizerWalletId = walletId;
            Amount = amount;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static FeeTransaction Create(Guid walletId, Money amount)
        {
            return new FeeTransaction(Guid.NewGuid(), walletId, amount);
        }
    }
}

