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
        public string Currency { get; private set; } = "XOF";

        public IReadOnlyCollection<WithdrawalRequest> Withdrawals => _withdrawals.AsReadOnly();
        public IReadOnlyCollection<FeeTransaction> Fees => _fees.AsReadOnly();

        // EF
        private OrganizerWallet() { }

        private OrganizerWallet(Guid id, Guid organizerId, string currency = "XOF")
            : base(id)
        {
            OrganizerId = organizerId;
            Currency = currency;
        }

        public static OrganizerWallet Create(Guid organizerId, string currency = "XOF")
        {
            return new OrganizerWallet(Guid.NewGuid(), organizerId, currency);
        }

        public void Credit(Money amount)
        {
            Balance = WalletBalance.Create(
                Balance.Available.Add(amount),
                Balance.Pending);
        }

        public void Debit(Money amount)
        {
            Balance = Balance.Withdraw(amount);
        }

        public void ReleasePending(Guid eventId, DateTimeOffset eventEndDate, DateTimeOffset now)
        {
            if (now < eventEndDate.AddHours(24))
            {
                return;
            }

            var pending = Balance.Pending;
            if (pending.Amount <= 0) return;

            Balance = Balance.MovePendingToAvailable(pending);
        }

        public void ReceiveRevenue(Money amount, Money platformFee, DateTimeOffset eventEndDate, DateTimeOffset now)
        {
            var net = Money.From(amount.Amount - platformFee.Amount, amount.Currency);

            _fees.Add(FeeTransaction.Create(Id, Guid.Empty, Guid.Empty, amount, platformFee, net));

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

        public WithdrawalRequest RequestWithdrawal(Money amount, Guid accountId = default)
        {
            Balance = Balance.Withdraw(amount);

            var withdrawal = WithdrawalRequest.Create(Id, amount, accountId);
            _withdrawals.Add(withdrawal);

            RaiseEvent(new WithdrawalRequested(Id, amount));
            return withdrawal;
        }

        public void MarkWithdrawalProcessed(Guid withdrawalId)
        {
            var w = _withdrawals.FirstOrDefault(x => x.Id == withdrawalId);
            if (w is null) return;

            w.MarkProcessed();
            RaiseEvent(new WithdrawalProcessed(Id, w.Id));
        }

        public void MarkWithdrawalFailed(Guid withdrawalId)
        {
            var w = _withdrawals.FirstOrDefault(x => x.Id == withdrawalId);

            if (w is null) return;

            w.MarkFailed();
            RaiseEvent(new WithdrawalFailed(Id, w.Id));
        }
    }

    public sealed class WithdrawalRequest : Entity<Guid>
    {
        public Guid OrganizerWalletId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public Guid AccountId { get; private set; }
        public WithdrawalStatus Status { get; private set; }
        public DateTimeOffset RequestedAt { get; private set; }
        public DateTimeOffset? ProcessedAt { get; private set; }

        // EF
        private WithdrawalRequest() { }

        private WithdrawalRequest(Guid id, Guid walletId, Money amount, Guid accountId)
            : base(id)
        {
            OrganizerWalletId = walletId;
            Amount = amount;
            AccountId = accountId;
            Status = WithdrawalStatus.Requested;
            RequestedAt = DateTimeOffset.UtcNow;
        }

        public static WithdrawalRequest Create(Guid walletId, Money amount, Guid accountId = default)
        {
            return new WithdrawalRequest(Guid.NewGuid(), walletId, amount, accountId);
        }

        public void MarkProcessed()
        {
            Status = WithdrawalStatus.Completed;
            ProcessedAt = DateTimeOffset.UtcNow;
        }

        public void MarkFailed()
        {
            Status = WithdrawalStatus.Failed;
        }
    }

    public sealed class FeeTransaction : Entity<Guid>
    {
        public Guid OrganizerWalletId { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid EventId { get; private set; }
        public Money GrossAmount { get; private set; } = null!;
        public Money PlatformFee { get; private set; } = null!;
        public Money NetAmount { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }

        // EF
        private FeeTransaction() { }

        private FeeTransaction(Guid id, Guid walletId, Guid orderId, Guid eventId, Money grossAmount, Money platformFee, Money netAmount)
            : base(id)
        {
            OrganizerWalletId = walletId;
            OrderId = orderId;
            EventId = eventId;
            GrossAmount = grossAmount;
            PlatformFee = platformFee;
            NetAmount = netAmount;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static FeeTransaction Create(Guid walletId, Guid orderId, Guid eventId, Money grossAmount, Money platformFee, Money netAmount)
        {
            return new FeeTransaction(Guid.NewGuid(), walletId, orderId, eventId, grossAmount, platformFee, netAmount);
        }
    }
}
