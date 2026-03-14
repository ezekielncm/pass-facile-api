using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Event
{
    public sealed record EventPublished(EventId Event) : Common.DomainEvent;
}
