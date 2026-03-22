using Domain.Common;
using Domain.DomainEvents.AccessControl;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccessControl
{
    public sealed class ScanSession : AggregateRoot<Guid>
    {
        private readonly List<ScanEvent> _events = [];

        public Guid EventId { get; private set; }
        public Guid AgentId { get; private set; }
        public DeviceId DeviceId { get; private set; } = null!;
        public DateTimeOffset StartedAt { get; private set; }
        public bool IsOffline { get; private set; }

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
            StartedAt = DateTimeOffset.UtcNow;
        }

        public static ScanSession Create(Guid eventId, Guid agentId, DeviceId deviceId, bool offline)
        {
            var session = new ScanSession(Guid.NewGuid(), eventId, agentId, deviceId, offline);
            return session;
        }

        public void StartOffline()
        {
            IsOffline = true;
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

        public ScanEvent RecordScan(string qrPayload, ScanResult result, DateTimeOffset now)
        {
            if (AgentId == Guid.Empty)
            {
                throw new BusinessRuleValidationException("ScanSession.AgentNotAssigned",
                    "Un agent doit être assigné pour scanner des tickets.");
            }

            var existing = _events.FirstOrDefault(e => e.QrPayload == qrPayload && e.Result.Status == ScanStatus.Valid);
            if (existing is not null)
            {
                var duplicate = ScanEvent.Create(Id, qrPayload, ScanResult.From("DUPLICATE"), now);
                _events.Add(duplicate);
                RaiseEvent(new DuplicateScanDetected(Id, qrPayload));
                return duplicate;
            }

            var scanEvent = ScanEvent.Create(Id, qrPayload, result, now);
            _events.Add(scanEvent);

            RaiseEvent(new TicketScanned(Id, qrPayload, result));
            return scanEvent;
        }

        public void Sync(IEnumerable<ScanEvent> scans, DateTimeOffset now)
        {
            if (!IsOffline)
            {
                return;
            }

            if (now - StartedAt > TimeSpan.FromHours(24))
            {
                throw new BusinessRuleValidationException("ScanSession.SyncTooLate",
                    "Une ScanSession hors ligne doit être synchronisée dans les 24h.");
            }

            foreach (var scan in scans)
            {
                scan.MarkSynced(now);
                _events.Add(scan);
            }

            RaiseEvent(new OfflineSyncCompleted(Id));
        }
    }

    public sealed class ScanEvent : Entity<Guid>
    {
        public Guid ScanSessionId { get; private set; }
        public string QrPayload { get; private set; } = null!;
        public ScanResult Result { get; private set; } = null!;
        public DateTimeOffset ScannedAt { get; private set; }
        public DateTimeOffset? SyncedAt { get; private set; }

        // EF
        private ScanEvent() { }

        private ScanEvent(Guid id, Guid scanSessionId, string qrPayload, ScanResult result, DateTimeOffset scannedAt)
            : base(id)
        {
            ScanSessionId = scanSessionId;
            QrPayload = qrPayload;
            Result = result;
            ScannedAt = scannedAt;
        }

        public static ScanEvent Create(Guid scanSessionId, string qrPayload, ScanResult result, DateTimeOffset at)
        {
            Guard.Against.NullOrEmpty(qrPayload, nameof(qrPayload));
            return new ScanEvent(Guid.NewGuid(), scanSessionId, qrPayload, result, at);
        }

        internal void MarkSynced(DateTimeOffset now)
        {
            SyncedAt = now;
        }
    }
}
