using Domain.Aggregates.AccessControl;
using Domain.Common;
using Domain.DomainEvents.AccessControl;
using Domain.Enums;
using Domain.ValueObjects;

namespace DomainUnitTest.Aggregates;

public class ScanSessionTests
{
    private static readonly Guid EventIdGuid = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();
    private static readonly DeviceId Device = DeviceId.From("device-1");

    private static ScanSession CreateValid(bool offline = false) =>
        ScanSession.Create(EventIdGuid, AgentId, Device, offline);

    /// <summary>
    /// Access domain events on ScanSession (the Events property is shadowed by scan events).
    /// </summary>
    private static IReadOnlyCollection<IDomainEvent> GetDomainEvents(ScanSession session) =>
        ((AggregateRoot<Guid>)session).Events;

    // --- Create ---

    [Fact]
    public void Create_SetsProperties()
    {
        var session = CreateValid();

        Assert.Equal(EventIdGuid, session.EventId);
        Assert.Equal(AgentId, session.AgentId);
        Assert.Equal(Device, session.DeviceId);
        Assert.False(session.IsOffline);
        Assert.Empty(session.Events);
    }

    [Fact]
    public void Create_WithOffline_SetsIsOffline()
    {
        var session = CreateValid(offline: true);

        Assert.True(session.IsOffline);
    }

    // --- StartOffline ---

    [Fact]
    public void StartOffline_SetsIsOfflineToTrue()
    {
        var session = CreateValid();

        session.StartOffline();

        Assert.True(session.IsOffline);
    }

    // --- AssignAgent ---

    [Fact]
    public void AssignAgent_UpdatesAgentAndRaisesEvent()
    {
        var session = CreateValid();
        var newAgent = Guid.NewGuid();

        session.AssignAgent(newAgent);

        Assert.Equal(newAgent, session.AgentId);
        Assert.Contains(GetDomainEvents(session), e => e is AgentAssigned);
    }

    // --- RevokeAgent ---

    [Fact]
    public void RevokeAgent_SetsAgentIdToEmptyAndRaisesEvent()
    {
        var session = CreateValid();

        session.RevokeAgent();

        Assert.Equal(Guid.Empty, session.AgentId);
        Assert.Contains(GetDomainEvents(session), e => e is AgentRevoked);
    }

    // --- RecordScan ---

    [Fact]
    public void RecordScan_WithValidPayload_AddsScanEventAndRaisesEvent()
    {
        var session = CreateValid();
        session.ClearEvents();
        var result = ScanResult.From("VALID");

        var scanEvent = session.RecordScan("qr-payload-1", result, DateTimeOffset.UtcNow);

        Assert.Single(session.Events);
        Assert.Equal("qr-payload-1", scanEvent.QrPayload);
        Assert.Contains(GetDomainEvents(session), e => e is TicketScanned);
    }

    [Fact]
    public void RecordScan_WithNoAgent_ThrowsBusinessRuleValidationException()
    {
        var session = CreateValid();
        session.RevokeAgent();

        Assert.Throws<BusinessRuleValidationException>(() =>
            session.RecordScan("qr-payload-1", ScanResult.From("VALID"), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void RecordScan_DuplicateValidPayload_ReturnsDuplicateAndRaisesEvent()
    {
        var session = CreateValid();
        session.RecordScan("qr-payload-1", ScanResult.From("VALID"), DateTimeOffset.UtcNow);
        session.ClearEvents();

        var dup = session.RecordScan("qr-payload-1", ScanResult.From("VALID"), DateTimeOffset.UtcNow);

        Assert.Equal(ScanStatus.AlreadyUsed, dup.Result.Status);
        Assert.Contains(GetDomainEvents(session), e => e is DuplicateScanDetected);
    }

    [Fact]
    public void RecordScan_SamePayloadButInvalidFirst_DoesNotTreatAsDuplicate()
    {
        var session = CreateValid();
        session.RecordScan("qr-1", ScanResult.From("INVALID"), DateTimeOffset.UtcNow);
        session.ClearEvents();

        var scan = session.RecordScan("qr-1", ScanResult.From("VALID"), DateTimeOffset.UtcNow);

        Assert.Equal(ScanStatus.Valid, scan.Result.Status);
        Assert.Contains(GetDomainEvents(session), e => e is TicketScanned);
    }

    // --- Sync ---

    [Fact]
    public void Sync_WhenOfflineAndWithin24h_AddsScanEvents()
    {
        var session = CreateValid(offline: true);
        var now = DateTimeOffset.UtcNow;
        var scans = new[]
        {
            ScanEvent.Create(session.Id, "qr-1", ScanResult.From("VALID"), now),
            ScanEvent.Create(session.Id, "qr-2", ScanResult.From("VALID"), now)
        };

        session.Sync(scans, now.AddHours(1));

        Assert.Equal(2, session.Events.Count);
        Assert.Contains(GetDomainEvents(session), e => e is OfflineSyncCompleted);
    }

    [Fact]
    public void Sync_WhenNotOffline_IsNoOp()
    {
        var session = CreateValid(offline: false);
        var now = DateTimeOffset.UtcNow;
        var scans = new[] { ScanEvent.Create(session.Id, "qr-1", ScanResult.From("VALID"), now) };
        session.ClearEvents();

        session.Sync(scans, now);

        Assert.Empty(session.Events);
        Assert.Empty(GetDomainEvents(session));
    }

    [Fact]
    public void Sync_After24Hours_ThrowsBusinessRuleValidationException()
    {
        var session = CreateValid(offline: true);
        var now = DateTimeOffset.UtcNow;
        var scans = new[] { ScanEvent.Create(session.Id, "qr-1", ScanResult.From("VALID"), now) };

        Assert.Throws<BusinessRuleValidationException>(() =>
            session.Sync(scans, now.AddHours(25)));
    }
}

public class ScanEventTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var sessionId = Guid.NewGuid();
        var result = ScanResult.From("VALID");
        var at = DateTimeOffset.UtcNow;

        var scanEvent = ScanEvent.Create(sessionId, "qr-payload", result, at);

        Assert.Equal(sessionId, scanEvent.ScanSessionId);
        Assert.Equal("qr-payload", scanEvent.QrPayload);
        Assert.Equal(result, scanEvent.Result);
        Assert.Equal(at, scanEvent.ScannedAt);
        Assert.Null(scanEvent.SyncedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullQrPayload_ThrowsArgumentException(string? payload)
    {
        Assert.Throws<ArgumentException>(() =>
            ScanEvent.Create(Guid.NewGuid(), payload!, ScanResult.From("VALID"), DateTimeOffset.UtcNow));
    }
}
