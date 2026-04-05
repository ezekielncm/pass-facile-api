using Domain.Aggregates.Finance;

namespace Application.Finance.DTOs;

public sealed record WalletDto(
    decimal AvailableBalance,
    decimal PendingBalance,
    string Currency,
    IReadOnlyCollection<TransactionDto> Transactions);

public sealed record TransactionDto(
    Guid Id,
    Guid OrderId,
    Guid EventId,
    decimal GrossAmount,
    decimal PlatformFee,
    decimal NetAmount,
    DateTimeOffset CreatedAt)
{
    public static TransactionDto FromDomain(FeeTransaction tx)
    {
        return new TransactionDto(
            tx.Id, tx.OrderId, tx.EventId,
            tx.GrossAmount.Amount, tx.PlatformFee.Amount, tx.NetAmount.Amount,
            tx.CreatedAt);
    }
}
