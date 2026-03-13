using Domain.Common;

namespace Domain.DomainEvents.Notifications
{
    public sealed record NotificationQueued(Guid NotificationRequestId) : Event;

    public sealed record NotificationSent(Guid NotificationRequestId) : Event;

    public sealed record NotificationFailed(Guid NotificationRequestId) : Event;

    public sealed record ReminderScheduled(Guid NotificationRequestId) : Event;
}

