using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Scan.DTOs;
using Domain.Aggregates.AccessControl;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scan.Commands.AssignAgent;

public sealed class AssignAgentCommandHandler
    : IRequestHandler<AssignAgentCommand, Result<AgentAssignmentDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISmsService _smsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignAgentCommandHandler> _logger;

    public AssignAgentCommandHandler(
        IEventRepository eventRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ISmsService smsService,
        IUnitOfWork unitOfWork,
        ILogger<AssignAgentCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _smsService = smsService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AgentAssignmentDto>> Handle(AssignAgentCommand cmd, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(cmd.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<AgentAssignmentDto>.Failure(Error.NotFound("Event", cmd.EventId));

        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        if (@event.OrganizerId != organizerId)
            return Result<AgentAssignmentDto>.Failure(Error.Validation("Accès refusé."));

        var agentPhone = new PhoneNumber(cmd.AgentPhone);
        var agent = await _userRepository.GetByPhoneNumberAsync(agentPhone, cancellationToken);

        Guid agentId;
        if (agent is null)
        {
            var newAgent = Domain.Aggregates.User.User.Register(agentPhone);
            await _userRepository.AddAsync(newAgent, cancellationToken);
            agentId = newAgent.Id.Value;
        }
        else
        {
            agentId = agent.Id.Value;
        }

        var assignment = AgentAssignment.Create(cmd.EventId, agentId, DateTimeOffset.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _smsService.SendAsync(cmd.AgentPhone, $"Vous êtes invité comme agent de scan pour l'événement {@event.Name}.");

        _logger.LogInformation("Agent {AgentId} assigné à l'événement {EventId}", agentId, cmd.EventId);
        return AgentAssignmentDto.FromDomain(assignment);
    }
}
