using Domain.Aggregates.Finance;

namespace Application.Finance.DTOs;

public sealed record WithdrawalDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ProcessedAt)
{
    public static WithdrawalDto FromDomain(WithdrawalRequest withdrawal)
    {
        return new WithdrawalDto(
            withdrawal.Id,
            withdrawal.Amount.Amount,
            withdrawal.Amount.Currency,
            withdrawal.Status.ToString(),
            withdrawal.RequestedAt,
            withdrawal.ProcessedAt);
    }
}
