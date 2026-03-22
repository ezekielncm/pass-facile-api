using Domain.Common;
using Domain.ValueObjects;

namespace Domain.DomainEvents.AccessControl
{
    public sealed record AgentAssigned(Guid ScanSessionId, Guid AgentId) : DomainEvent;

    public sealed record AgentRevoked(Guid ScanSessionId, Guid AgentId) : DomainEvent;

    public sealed record TicketScanned(Guid ScanSessionId, string QrPayload, ScanResult Result) : DomainEvent;

    public sealed record DuplicateScanDetected(Guid ScanSessionId, string QrPayload) : DomainEvent;

    public sealed record OfflineSyncCompleted(Guid ScanSessionId) : DomainEvent;
}
