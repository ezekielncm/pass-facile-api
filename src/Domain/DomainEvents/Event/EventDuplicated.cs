using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Event
{
    public sealed record EventDuplicated(EventId SourceEventId, EventId NewEventId) : Common.DomainEvent;
}
