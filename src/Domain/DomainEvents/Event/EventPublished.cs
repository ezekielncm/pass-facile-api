using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Event
{
    public sealed record EventPublished(EventId EventId) : Common.Event;
}
