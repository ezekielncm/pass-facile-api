using Domain.Aggregates.Ticketing;

namespace Application.Tickets.DTOs;

public sealed record TicketDto(
    Guid Id,
    string Reference,
    Guid OrderId,
    Guid EventId,
    Guid CategoryId,
    string BuyerPhone,
    string Status,
    DateTimeOffset IssuedAt,
    string? QrPayload)
{
    public static TicketDto FromDomain(Ticket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.Reference.Value,
            ticket.OrderId,
            ticket.EventId,
            ticket.CategoryId,
            ticket.BuyerPhone.Value,
            ticket.Status.ToString(),
            ticket.IssuedAt,
            ticket.QRCode?.Payload?.Encode());
    }
}
