using Domain.Aggregates.Sales;

namespace Application.Orders.DTOs;

public sealed record PaymentDto(
    Guid Id,
    string Provider,
    string PhoneNumber,
    decimal Amount,
    string Status,
    DateTimeOffset InitiatedAt,
    DateTimeOffset? ConfirmedAt)
{
    public static PaymentDto FromDomain(Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.Provider.ToString(),
            payment.PhoneNumber.Value,
            payment.Amount.Amount,
            payment.Status.ToString(),
            payment.InitiatedAt,
            payment.ConfirmedAt);
    }
}
