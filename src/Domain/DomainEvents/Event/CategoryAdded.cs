using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Event
{
    public sealed record CategoryAdded(EventId Event, Guid TicketCategoryId) : Common.DomainEvent;
}
