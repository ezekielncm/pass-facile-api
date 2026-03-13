using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Event
{
    public sealed record EventCreated(EventId EventId) : Common.Event;
}
