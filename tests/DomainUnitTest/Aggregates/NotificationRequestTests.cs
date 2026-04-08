using Domain.Aggregates.Notifications;
using Domain.Common;
using Domain.DomainEvents.Notifications;
using Domain.Enums;
using Domain.ValueObjects;

namespace DomainUnitTest.Aggregates;

public class NotificationRequestTests
{
    private static readonly PhoneNumber RecipientPhone = new("226", "70000001");
    private static readonly Channel SmsChannel = Channel.From("SMS");

    private static NotificationRequest QueueValid(Dictionary<string, string>? payload = null) =>
        NotificationRequest.Queue(RecipientPhone, SmsChannel, "welcome-template", payload);

    // --- Queue ---

    [Fact]
    public void Queue_WithValidData_SetsPropertiesAndQueuedStatus()
    {
        var notif = QueueValid();

        Assert.Equal(RecipientPhone, notif.RecipientPhone);
        Assert.Equal(SmsChannel, notif.Channel);
        Assert.Equal("welcome-template", notif.TemplateId);
        Assert.Equal(NotificationStatus.Queued, notif.Status);
        Assert.Null(notif.ScheduledAt);
        Assert.Empty(notif.Attempts);
    }

    [Fact]
    public void Queue_WithPayload_SetsPayload()
    {
        var payload = new Dictionary<string, string> { ["name"] = "John", ["event"] = "Concert" };

        var notif = QueueValid(payload);

        Assert.Equal(2, notif.Payload.Count);
        Assert.Equal("John", notif.Payload["name"]);
    }

    [Fact]
    public void Queue_RaisesNotificationQueued()
    {
        var notif = QueueValid();

        Assert.Contains(notif.Events, e => e is NotificationQueued);
    }

    [Fact]
    public void Queue_WithNullPhone_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            NotificationRequest.Queue(null!, SmsChannel, "template"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Queue_WithEmptyOrNullTemplateId_ThrowsArgumentException(string? templateId)
    {
        Assert.Throws<ArgumentException>(() =>
            NotificationRequest.Queue(RecipientPhone, SmsChannel, templateId!));
    }

    // --- Send ---

    [Fact]
    public void Send_RecordsSuccessfulAttemptAndSetsSentStatus()
    {
        var notif = QueueValid();
        notif.ClearEvents();

        notif.Send();

        Assert.Equal(NotificationStatus.Sent, notif.Status);
        Assert.Single(notif.Attempts);
        Assert.True(notif.Attempts.First().Success);
        Assert.Contains(notif.Events, e => e is NotificationSent);
    }

    // --- Schedule ---

    [Fact]
    public void Schedule_SetsScheduledAtAndRaisesEvent()
    {
        var notif = QueueValid();
        notif.ClearEvents();
        var scheduledTime = DateTimeOffset.UtcNow.AddHours(2);

        notif.Schedule(scheduledTime);

        Assert.Equal(scheduledTime, notif.ScheduledAt);
        Assert.Contains(notif.Events, e => e is ReminderScheduled);
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_SetsCancelledStatus()
    {
        var notif = QueueValid();

        notif.Cancel();

        Assert.Equal(NotificationStatus.Cancelled, notif.Status);
    }

    // --- RecordAttempt ---

    [Fact]
    public void RecordAttempt_SuccessfulAttempt_SetsSentStatus()
    {
        var notif = QueueValid();
        notif.ClearEvents();

        notif.RecordAttempt(true);

        Assert.Equal(NotificationStatus.Sent, notif.Status);
        Assert.Single(notif.Attempts);
        Assert.Contains(notif.Events, e => e is NotificationSent);
    }

    [Fact]
    public void RecordAttempt_FailedAttempt_SetsFailedStatus()
    {
        var notif = QueueValid();
        notif.ClearEvents();

        notif.RecordAttempt(false, "Provider unreachable");

        Assert.Equal(NotificationStatus.Failed, notif.Status);
        Assert.Single(notif.Attempts);
        Assert.Equal("Provider unreachable", notif.Attempts.First().ErrorMessage);
        Assert.Contains(notif.Events, e => e is NotificationFailed);
    }

    [Fact]
    public void RecordAttempt_IncrementsAttemptNumber()
    {
        var notif = QueueValid();
        notif.RecordAttempt(false, "Error 1");
        notif.RecordAttempt(false, "Error 2");

        Assert.Equal(2, notif.Attempts.Count);
        Assert.Equal(1, notif.Attempts.First().AttemptNumber);
        Assert.Equal(2, notif.Attempts.Last().AttemptNumber);
    }

    [Fact]
    public void RecordAttempt_AfterMaxAttempts_ThrowsBusinessRuleValidationException()
    {
        var notif = QueueValid();
        notif.RecordAttempt(false, "Error 1");
        notif.RecordAttempt(false, "Error 2");
        notif.RecordAttempt(false, "Error 3");

        Assert.Throws<BusinessRuleValidationException>(() =>
            notif.RecordAttempt(false, "Error 4"));
    }

    [Fact]
    public void RecordAttempt_ThirdAttemptSucceeds_SetsSentStatus()
    {
        var notif = QueueValid();
        notif.RecordAttempt(false, "Error 1");
        notif.RecordAttempt(false, "Error 2");
        notif.ClearEvents();

        notif.RecordAttempt(true);

        Assert.Equal(NotificationStatus.Sent, notif.Status);
        Assert.Equal(3, notif.Attempts.Count);
    }
}

public class DeliveryAttemptTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var requestId = Guid.NewGuid();

        var attempt = DeliveryAttempt.Create(requestId, 1, true);

        Assert.Equal(requestId, attempt.NotificationRequestId);
        Assert.Equal(1, attempt.AttemptNumber);
        Assert.True(attempt.Success);
        Assert.Null(attempt.ErrorMessage);
    }

    [Fact]
    public void Create_WithErrorMessage_SetsErrorMessage()
    {
        var attempt = DeliveryAttempt.Create(Guid.NewGuid(), 2, false, "timeout");

        Assert.False(attempt.Success);
        Assert.Equal("timeout", attempt.ErrorMessage);
    }
}
