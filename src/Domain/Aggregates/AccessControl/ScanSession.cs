using Domain.Common;
using Domain.DomainEvents.AccessControl;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccessControl
{
    public sealed class ScanSession : AggregateRoot<Guid>
    {
        private readonly List<ScanEvent> _events = [];

        public Guid EventId { get; private set; }
        public Guid AgentId { get; private set; }
        public DeviceId DeviceId { get; private set; } = null!;
        public bool IsOffline { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? SyncedAt { get; private set; }

        public IReadOnlyCollection<ScanEvent> Events => _events.AsReadOnly();

        // EF
        private ScanSession() { }

        private ScanSession(Guid id, Guid eventId, Guid agentId, DeviceId deviceId, bool isOffline)
            : base(id)
        {
            EventId = eventId;
            AgentId = agentId;
            DeviceId = deviceId;
            IsOffline = isOffline;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static ScanSession Create(Guid eventId, Guid agentId, DeviceId deviceId, bool offline)
        {
            var session = new ScanSession(Guid.NewGuid(), eventId, agentId, deviceId, offline);
            return session;
        }

        public void AssignAgent(Guid agentId)
        {
            AgentId = agentId;
            RaiseEvent(new AgentAssigned(Id, agentId));
        }

        public void RevokeAgent()
        {
            var previous = AgentId;
            AgentId = Guid.Empty;
            RaiseEvent(new AgentRevoked(Id, previous));
        }

        public ScanEvent RecordScan(Guid ticketId, ScanResult result, DateTimeOffset now)
        {
            // Invariant: un agent ne peut scanner que les événements auxquels il est assigné
            if (AgentId == Guid.Empty)
            {
                throw new BusinessRuleValidationException("ScanSession.AgentNotAssigned",
                    "Un agent doit être assigné pour scanner des tickets.");
            }

            var existing = _events.FirstOrDefault(e => e.TicketId == ticketId && e.Result.Value == "VALID");
            if (existing is not null)
            {
                // Duplicate scan detected
                var duplicate = ScanEvent.Create(Id, ticketId, ScanResult.From("DUPLICATE"), now);
                _events.Add(duplicate);
                RaiseEvent(new DuplicateScanDetected(Id, ticketId));
                return duplicate;
            }

            // Un scan invalide ne modifie pas l'état du Ticket → ici, on se contente d’enregistrer.
            var scanEvent = ScanEvent.Create(Id, ticketId, result, now);
            _events.Add(scanEvent);

            RaiseEvent(new TicketScanned(Id, ticketId, result));
            return scanEvent;
        }

        public void MarkOfflineSyncCompleted(DateTimeOffset now)
        {
            if (!IsOffline)
            {
                return;
            }

            if (now - CreatedAt > TimeSpan.FromHours(24))
            {
                throw new BusinessRuleValidationException("ScanSession.SyncTooLate",
                    "Une ScanSession hors ligne doit être synchronisée dans les 24h.");
            }

            SyncedAt = now;
            RaiseEvent(new OfflineSyncCompleted(Id));
        }
    }

    public sealed class ScanEvent : Entity<Guid>
    {
        public Guid ScanSessionId { get; private set; }
        public Guid TicketId { get; private set; }
        public ScanResult Result { get; private set; } = null!;
        public DateTimeOffset ScannedAt { get; private set; }

        // EF
        private ScanEvent() { }

        private ScanEvent(Guid id, Guid scanSessionId, Guid ticketId, ScanResult result, DateTimeOffset scannedAt)
            : base(id)
        {
            ScanSessionId = scanSessionId;
            TicketId = ticketId;
            Result = result;
            ScannedAt = scannedAt;
        }

        public static ScanEvent Create(Guid scanSessionId, Guid ticketId, ScanResult result, DateTimeOffset at)
        {
            return new ScanEvent(Guid.NewGuid(), scanSessionId, ticketId, result, at);
        }
    }
}

