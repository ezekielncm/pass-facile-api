using Domain.Aggregates.Sales;

namespace Application.Orders.DTOs;

public sealed record RefundDto(
    Guid Id,
    decimal Amount,
    string Reason,
    string Status,
    DateTimeOffset? ProcessedAt)
{
    public static RefundDto FromDomain(Refund refund)
    {
        return new RefundDto(
            refund.Id,
            refund.Amount.Amount,
            refund.Reason,
            refund.Status.ToString(),
            refund.ProcessedAt);
    }
}
