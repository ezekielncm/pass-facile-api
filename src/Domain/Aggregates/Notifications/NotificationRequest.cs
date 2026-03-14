using Domain.Common;
using Domain.DomainEvents.Notifications;
using Domain.ValueObjects;

namespace Domain.Aggregates.Notifications
{
    public sealed class NotificationRequest : AggregateRoot<Guid>
    {
        private readonly List<DeliveryAttempt> _attempts = [];

        public Channel Channel { get; private set; } = null!;
        public MessageTemplate Template { get; private set; } = null!;
        public RecipientContact Recipient { get; private set; } = null!;
        public bool IsOptOut { get; private set; }
        public bool IsQueued { get; private set; }

        public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

        // EF
        private NotificationRequest() { }

        private NotificationRequest(Guid id, Channel channel, MessageTemplate template, RecipientContact recipient)
            : base(id)
        {
            Channel = channel;
            Template = template;
            Recipient = recipient;
            IsQueued = true;

            RaiseEvent(new NotificationQueued(Id));
        }

        public static NotificationRequest Queue(Channel channel, MessageTemplate template, RecipientContact recipient)
        {
            return new NotificationRequest(Guid.NewGuid(), channel, template, recipient);
        }

        public void RecordAttempt(bool success)
        {
            if (_attempts.Count >= 3)
            {
                throw new BusinessRuleValidationException("Notification.MaxAttempts",
                    "Maximum 3 tentatives d'envoi par notification.");
            }

            var attempt = DeliveryAttempt.Create(Id, success);
            _attempts.Add(attempt);

            if (success)
            {
                RaiseEvent(new NotificationSent(Id));
            }
            else
            {
                RaiseEvent(new NotificationFailed(Id));
            }
        }

        public void ScheduleReminder()
        {
            if (IsOptOut)
            {
                return;
            }

            RaiseEvent(new ReminderScheduled(Id));
        }

        public void OptOut()
        {
            IsOptOut = true;
        }
    }

    public sealed class DeliveryAttempt : Entity<Guid>
    {
        public Guid NotificationRequestId { get; private set; }
        public bool Success { get; private set; }
        public DateTimeOffset AttemptedAt { get; private set; }

        // EF
        private DeliveryAttempt() { }

        private DeliveryAttempt(Guid id, Guid notificationRequestId, bool success)
            : base(id)
        {
            NotificationRequestId = notificationRequestId;
            Success = success;
            AttemptedAt = DateTimeOffset.UtcNow;
        }

        public static DeliveryAttempt Create(Guid requestId, bool success)
        {
            return new DeliveryAttempt(Guid.NewGuid(), requestId, success);
        }
    }
}

