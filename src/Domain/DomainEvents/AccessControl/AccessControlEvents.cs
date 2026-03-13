using Domain.Common;
using Domain.ValueObjects;

namespace Domain.DomainEvents.AccessControl
{
    public sealed record AgentAssigned(Guid ScanSessionId, Guid AgentId) : Event;

    public sealed record AgentRevoked(Guid ScanSessionId, Guid AgentId) : Event;

    public sealed record TicketScanned(Guid ScanSessionId, Guid TicketId, ScanResult Result) : Event;

    public sealed record DuplicateScanDetected(Guid ScanSessionId, Guid TicketId) : Event;

    public sealed record OfflineSyncCompleted(Guid ScanSessionId) : Event;
}

