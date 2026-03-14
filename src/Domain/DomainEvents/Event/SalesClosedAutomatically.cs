using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Event
{
    public sealed record SalesClosedAutomatically(EventId Id) : Common.DomainEvent;
}
