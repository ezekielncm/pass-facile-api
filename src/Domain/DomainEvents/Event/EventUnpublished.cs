using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Event
{
    public sealed record EventUnpublished(EventId EventId) : Common.Event;
}
