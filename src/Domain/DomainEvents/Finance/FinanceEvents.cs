using Domain.Common;
using Domain.ValueObjects;

namespace Domain.DomainEvents.Finance
{
    public sealed record RevenueReceived(Guid WalletId, Money NetAmount) : Event;

    public sealed record WithdrawalRequested(Guid WalletId, Money Amount) : Event;

    public sealed record WithdrawalProcessed(Guid WalletId, Guid WithdrawalId) : Event;

    public sealed record WithdrawalFailed(Guid WalletId, Guid WithdrawalId) : Event;
}

