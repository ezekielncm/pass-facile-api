using Domain.Aggregates.Sales;

namespace Application.Orders.DTOs;

public sealed record OrderDto(
    Guid Id,
    string BuyerPhone,
    Guid EventId,
    Guid CategoryId,
    int Quantity,
    decimal Subtotal,
    decimal Fees,
    decimal Total,
    string Status,
    DateTimeOffset ReservedUntil,
    DateTimeOffset CreatedAt)
{
    public static OrderDto FromDomain(Order order)
    {
        return new OrderDto(
            order.Id.Value,
            order.BuyerPhone.Value,
            order.EventId,
            order.CategoryId,
            order.Quantity,
            order.Subtotal.Amount,
            order.Fees.Amount,
            order.Total.Amount,
            order.Status.ToString(),
            order.ReservedUntil,
            order.CreatedAt);
    }
}
