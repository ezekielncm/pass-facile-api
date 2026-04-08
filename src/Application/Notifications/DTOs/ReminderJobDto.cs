namespace Application.Notifications.DTOs;

public sealed record ReminderJobDto(
    Guid JobId,
    Guid EventId,
    DateTimeOffset? ScheduledAt,
    string Status);
