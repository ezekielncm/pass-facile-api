using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events.Queries.GetEvents;

public sealed class GetEventsQueryHandler
    : IRequestHandler<GetEventsQuery, Result<PagedResult<EventDto>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetEventsQueryHandler> _logger;

    public GetEventsQueryHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        ILogger<GetEventsQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<EventDto>>> Handle(GetEventsQuery query, CancellationToken cancellationToken)
    {
        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        var events = await _eventRepository.GetByOrganizerIdAsync(organizerId, cancellationToken);

        if (events is null || events.Count == 0)
        {
            _logger.LogWarning("Aucun événement trouvé pour l'organisateur {OrganizerId}", organizerId);
            return new PagedResult<EventDto>([], 0, query.Page, query.PageSize);
        }

        var filtered = events.AsEnumerable();
        if (!string.IsNullOrEmpty(query.Status))
        {
            if (Enum.TryParse<Domain.Enums.EventStatus>(query.Status, true, out var status))
                filtered = filtered.Where(e => e.Status == status);
        }

        var sorted = filtered.OrderByDescending(e => e.StartDate).ToList();
        var eventDtos = sorted.Select(EventDto.FromDomain).ToList();

        return PagedResult<EventDto>.Create(eventDtos, query.Page, query.PageSize);
    }
}
