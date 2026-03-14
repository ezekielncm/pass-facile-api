using Domain.Common;
using Domain.ValueObjects;

namespace Domain.DomainEvents.Finance
{
    public sealed record RevenueReceived(Guid WalletId, Money NetAmount) : DomainEvent;

    public sealed record WithdrawalRequested(Guid WalletId, Money Amount) : DomainEvent;

    public sealed record WithdrawalProcessed(Guid WalletId, Guid WithdrawalId) : DomainEvent;

    public sealed record WithdrawalFailed(Guid WalletId, Guid WithdrawalId) : DomainEvent;
}

