using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands.UpdateEvent;

public sealed class UpdateEventCommandHandler
    : IRequestHandler<UpdateEventCommand, Result<EventDto>>
{
    private readonly ILogger<UpdateEventCommandHandler> _logger;
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEventCommandHandler(
        ILogger<UpdateEventCommandHandler> logger,
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EventDto>> Handle(UpdateEventCommand cmd, CancellationToken cancellationToken)
    {
        var id = Domain.ValueObjects.Identities.EventId.From(cmd.EventId);
        var @event = await _eventRepository.GetByIdAsync(id, cancellationToken);

        if (@event is null)
        {
            _logger.LogWarning("Événement {EventId} introuvable", cmd.EventId);
            return Result<EventDto>.Failure(Error.NotFound("Event", cmd.EventId));
        }

        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        if (@event.OrganizerId != organizerId)
            return Result<EventDto>.Failure(Error.Validation("Seul l'organisateur propriétaire peut modifier cet événement."));

        if (@event.Status == Domain.Enums.EventStatus.Finished)
            return Result<EventDto>.Failure(Error.Validation("Impossible de modifier un événement terminé."));

        // TODO: Apply property updates once domain mutation methods are added.
        // The Event aggregate currently uses private setters; a domain Update method
        // (e.g., Event.UpdateDetails) should be introduced to mutate properties.

        await _eventRepository.UpdateAsync(@event, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Événement {EventId} mis à jour", cmd.EventId);
        return EventDto.FromDomain(@event);
    }
}
