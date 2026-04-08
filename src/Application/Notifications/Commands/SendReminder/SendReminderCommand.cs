using Application.Common.Models;
using Application.Notifications.DTOs;
using MediatR;

namespace Application.Notifications.Commands.SendReminder;

public sealed record SendReminderCommand(
    Guid EventId,
    string? Message,
    DateTimeOffset? ScheduledAt) : IRequest<Result<ReminderJobDto>>;
