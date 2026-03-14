using Domain.Common;

namespace Domain.DomainEvents.Notifications
{
    public sealed record NotificationQueued(Guid NotificationRequestId) : DomainEvent;

    public sealed record NotificationSent(Guid NotificationRequestId) : DomainEvent;

    public sealed record NotificationFailed(Guid NotificationRequestId) : DomainEvent;

    public sealed record ReminderScheduled(Guid NotificationRequestId) : DomainEvent;
}

