using Domain.Common;
using Domain.ValueObjects;

namespace Domain.DomainEvents.Ticketing
{
    public sealed record TicketIssued(Guid TicketId, TicketReference Reference) : Event;

    public sealed record QRCodeGenerated(Guid TicketId, QRCodePayload Payload) : Event;

    public sealed record TicketDelivered(Guid TicketId) : Event;

    public sealed record TicketRevoked(Guid TicketId) : Event;
}

