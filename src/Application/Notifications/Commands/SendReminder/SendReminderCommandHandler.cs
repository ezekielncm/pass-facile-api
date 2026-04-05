using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Notifications.DTOs;
using Domain.Aggregates.Notifications;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Notifications.Commands.SendReminder;

public sealed class SendReminderCommandHandler
    : IRequestHandler<SendReminderCommand, Result<ReminderJobDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly INotificationRequestRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendReminderCommandHandler> _logger;

    public SendReminderCommandHandler(
        IEventRepository eventRepository,
        ITicketRepository ticketRepository,
        INotificationRequestRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<SendReminderCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _ticketRepository = ticketRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ReminderJobDto>> Handle(SendReminderCommand cmd, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(cmd.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<ReminderJobDto>.Failure(Error.NotFound("Event", cmd.EventId));

        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        if (@event.OrganizerId != organizerId)
            return Result<ReminderJobDto>.Failure(Error.Validation("Accès refusé."));

        var tickets = await _ticketRepository.GetByEventIdAsync(cmd.EventId, cancellationToken);
        var phones = tickets.Select(t => t.BuyerPhone).Distinct().ToList();

        var channel = Channel.From("SMS");
        var message = cmd.Message ?? $"Rappel : votre événement {@event.Name} approche !";
        var scheduledAt = cmd.ScheduledAt ?? DateTimeOffset.UtcNow;

        // Create notification requests for each buyer
        foreach (var phone in phones)
        {
            var payload = new Dictionary<string, string> { { "message", message } };
            var notification = NotificationRequest.Queue(phone, channel, "EVENT_REMINDER", payload);

            if (cmd.ScheduledAt.HasValue)
                notification.Schedule(cmd.ScheduledAt.Value);

            await _notificationRepository.AddAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Rappel planifié pour {Count} acheteurs de l'événement {EventId}", phones.Count, cmd.EventId);
        return new ReminderJobDto(Guid.NewGuid(), cmd.EventId, scheduledAt, "Queued");
    }
}
