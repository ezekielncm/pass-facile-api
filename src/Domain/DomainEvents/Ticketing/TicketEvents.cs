using Domain.Common;
using Domain.ValueObjects;

namespace Domain.DomainEvents.Ticketing
{
    public sealed record TicketIssued(Guid TicketId, TicketReference Reference) : DomainEvent;

    public sealed record QRCodeGenerated(Guid TicketId, QRCodePayload Payload) : DomainEvent;

    public sealed record TicketDelivered(Guid TicketId) : DomainEvent;

    public sealed record TicketRevoked(Guid TicketId) : DomainEvent;
}

