using Domain.Common;
using Domain.DomainEvents.Notifications;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Aggregates.Notifications
{
    public sealed class NotificationRequest : AggregateRoot<Guid>
    {
        private readonly List<DeliveryAttempt> _attempts = [];

        public PhoneNumber RecipientPhone { get; private set; } = null!;
        public Channel Channel { get; private set; } = null!;
        public string TemplateId { get; private set; } = null!;
        public Dictionary<string, string> Payload { get; private set; } = new();
        public DateTimeOffset? ScheduledAt { get; private set; }
        public NotificationStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

        // EF
        private NotificationRequest() { }

        private NotificationRequest(Guid id, PhoneNumber recipientPhone, Channel channel, string templateId, Dictionary<string, string>? payload)
            : base(id)
        {
            RecipientPhone = recipientPhone;
            Channel = channel;
            TemplateId = templateId;
            Payload = payload ?? new();
            Status = NotificationStatus.Queued;
            CreatedAt = DateTimeOffset.UtcNow;

            RaiseEvent(new NotificationQueued(Id));
        }

        public static NotificationRequest Queue(PhoneNumber recipientPhone, Channel channel, string templateId, Dictionary<string, string>? payload = null)
        {
            Guard.Against.Null(recipientPhone, nameof(recipientPhone));
            Guard.Against.NullOrEmpty(templateId, nameof(templateId));
            return new NotificationRequest(Guid.NewGuid(), recipientPhone, channel, templateId, payload);
        }

        public void Send()
        {
            RecordAttempt(true);
        }

        public void Schedule(DateTimeOffset at)
        {
            ScheduledAt = at;
            RaiseEvent(new ReminderScheduled(Id));
        }

        public void Cancel()
        {
            Status = NotificationStatus.Cancelled;
        }

        public void RecordAttempt(bool success, string? errorMessage = null)
        {
            if (_attempts.Count >= 3)
            {
                throw new BusinessRuleValidationException("Notification.MaxAttempts",
                    "Maximum 3 tentatives d'envoi par notification.");
            }

            var attemptNumber = _attempts.Count + 1;
            var attempt = DeliveryAttempt.Create(Id, attemptNumber, success, errorMessage);
            _attempts.Add(attempt);

            if (success)
            {
                Status = NotificationStatus.Sent;
                RaiseEvent(new NotificationSent(Id));
            }
            else
            {
                Status = NotificationStatus.Failed;
                RaiseEvent(new NotificationFailed(Id));
            }
        }
    }

    public sealed class DeliveryAttempt : Entity<Guid>
    {
        public Guid NotificationRequestId { get; private set; }
        public int AttemptNumber { get; private set; }
        public DateTimeOffset SentAt { get; private set; }
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }

        // EF
        private DeliveryAttempt() { }

        private DeliveryAttempt(Guid id, Guid notificationRequestId, int attemptNumber, bool success, string? errorMessage)
            : base(id)
        {
            NotificationRequestId = notificationRequestId;
            AttemptNumber = attemptNumber;
            Success = success;
            ErrorMessage = errorMessage;
            SentAt = DateTimeOffset.UtcNow;
        }

        public static DeliveryAttempt Create(Guid requestId, int attemptNumber, bool success, string? errorMessage = null)
        {
            return new DeliveryAttempt(Guid.NewGuid(), requestId, attemptNumber, success, errorMessage);
        }
    }
}
