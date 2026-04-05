using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scan.Commands.RevokeAgent;

public sealed class RevokeAgentCommandHandler
    : IRequestHandler<RevokeAgentCommand, Result<bool>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RevokeAgentCommandHandler> _logger;

    public RevokeAgentCommandHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<RevokeAgentCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RevokeAgentCommand cmd, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(cmd.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<bool>.Failure(Error.NotFound("Event", cmd.EventId));

        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        if (@event.OrganizerId != organizerId)
            return Result<bool>.Failure(Error.Validation("Accès refusé."));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Agent {AgentId} révoqué de l'événement {EventId}", cmd.AgentId, cmd.EventId);
        return true;
    }
}
