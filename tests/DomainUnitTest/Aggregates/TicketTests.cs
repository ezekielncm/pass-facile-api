using Domain.Aggregates.Ticketing;
using Domain.Common;
using Domain.DomainEvents.Ticketing;
using Domain.Enums;
using Domain.ValueObjects;

namespace DomainUnitTest.Aggregates;

public class TicketTests
{
    private static readonly PhoneNumber BuyerPhone = new("226", "70000001");
    private static readonly TicketReference Reference = TicketReference.From("TKT-ABC-123");

    private static QRCodePayload MakePayload() =>
        QRCodePayload.Create("TKT-ABC-123", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "sig123");

    private static Ticket IssueValid() =>
        Ticket.Issue(Reference, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BuyerPhone, "John Doe");

    // --- Issue ---

    [Fact]
    public void Issue_WithValidData_SetsPropertiesAndIssuedStatus()
    {
        var orderId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var ticket = Ticket.Issue(Reference, orderId, eventId, categoryId, BuyerPhone, "John Doe");

        Assert.Equal(orderId, ticket.OrderId);
        Assert.Equal(eventId, ticket.EventId);
        Assert.Equal(categoryId, ticket.CategoryId);
        Assert.Equal(BuyerPhone, ticket.BuyerPhone);
        Assert.Equal("John Doe", ticket.BuyerName);
        Assert.Equal(Reference, ticket.Reference);
        Assert.Equal(TicketStatus.Issued, ticket.Status);
        Assert.Null(ticket.QRCode);
    }

    [Fact]
    public void Issue_RaisesTicketIssued()
    {
        var ticket = IssueValid();

        Assert.Contains(ticket.Events, e => e is TicketIssued);
    }

    [Fact]
    public void Issue_WithNullPhone_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Ticket.Issue(Reference, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null!, "John"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Issue_WithEmptyOrNullBuyerName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            Ticket.Issue(Reference, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BuyerPhone, name!));
    }

    // --- AttachQrCode ---

    [Fact]
    public void AttachQrCode_WhenIssued_AttachesCodeAndRaisesEvent()
    {
        var ticket = IssueValid();
        ticket.ClearEvents();
        var payload = MakePayload();

        ticket.AttachQrCode(payload);

        Assert.NotNull(ticket.QRCode);
        Assert.Equal(payload, ticket.QRCode!.Payload);
        Assert.Contains(ticket.Events, e => e is QRCodeGenerated);
    }

    [Fact]
    public void AttachQrCode_WhenRevoked_ThrowsBusinessRuleValidationException()
    {
        var ticket = IssueValid();
        ticket.Revoke();

        Assert.Throws<BusinessRuleValidationException>(() => ticket.AttachQrCode(MakePayload()));
    }

    [Fact]
    public void AttachQrCode_WhenUsed_ThrowsBusinessRuleValidationException()
    {
        var ticket = IssueValid();
        ticket.MarkUsed();

        Assert.Throws<BusinessRuleValidationException>(() => ticket.AttachQrCode(MakePayload()));
    }

    // --- MarkDelivered ---

    [Fact]
    public void MarkDelivered_WhenIssued_RaisesTicketDelivered()
    {
        var ticket = IssueValid();
        ticket.ClearEvents();

        ticket.MarkDelivered();

        Assert.Contains(ticket.Events, e => e is TicketDelivered);
    }

    [Fact]
    public void MarkDelivered_WhenNotIssued_IsNoOp()
    {
        var ticket = IssueValid();
        ticket.MarkUsed(); // now Used
        ticket.ClearEvents();

        ticket.MarkDelivered();

        Assert.Empty(ticket.Events);
    }

    // --- Revoke ---

    [Fact]
    public void Revoke_SetsRevokedStatusAndRaisesEvent()
    {
        var ticket = IssueValid();
        ticket.ClearEvents();

        ticket.Revoke();

        Assert.Equal(TicketStatus.Revoked, ticket.Status);
        Assert.Contains(ticket.Events, e => e is TicketRevoked);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_IsIdempotent()
    {
        var ticket = IssueValid();
        ticket.Revoke();
        ticket.ClearEvents();

        ticket.Revoke();

        Assert.Equal(TicketStatus.Revoked, ticket.Status);
        Assert.Empty(ticket.Events);
    }

    // --- MarkUsed ---

    [Fact]
    public void MarkUsed_WhenIssued_SetsUsedStatus()
    {
        var ticket = IssueValid();

        ticket.MarkUsed();

        Assert.Equal(TicketStatus.Used, ticket.Status);
    }

    [Fact]
    public void MarkUsed_WhenRevoked_ThrowsBusinessRuleValidationException()
    {
        var ticket = IssueValid();
        ticket.Revoke();

        Assert.Throws<BusinessRuleValidationException>(() => ticket.MarkUsed());
    }

    [Fact]
    public void MarkUsed_WhenAlreadyUsed_ThrowsBusinessRuleValidationException()
    {
        var ticket = IssueValid();
        ticket.MarkUsed();

        Assert.Throws<BusinessRuleValidationException>(() => ticket.MarkUsed());
    }
}

public class QRCodeTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var ticketId = Guid.NewGuid();
        var payload = QRCodePayload.Create("TKT-001", "evt-1", "cat-1", "sig");

        var qr = QRCode.Create(ticketId, payload);

        Assert.Equal(ticketId, qr.TicketId);
        Assert.Equal(payload, qr.Payload);
        Assert.Null(qr.ExpiresAt);
    }

    [Fact]
    public void Create_WithExpiry_SetsExpiresAt()
    {
        var expiry = DateTimeOffset.UtcNow.AddHours(2);
        var payload = QRCodePayload.Create("TKT-001", "evt-1", "cat-1", "sig");

        var qr = QRCode.Create(Guid.NewGuid(), payload, expiry);

        Assert.Equal(expiry, qr.ExpiresAt);
    }
}
