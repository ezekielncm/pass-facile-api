using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events.Queries.GetEventPublish;

public sealed class GetEventPublishQueryHandler
    : IRequestHandler<GetEventPublishQuery, Result<EventPublishDto>>
{
    private readonly ILogger<GetEventPublishQueryHandler> _logger;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public GetEventPublishQueryHandler(
        ILogger<GetEventPublishQueryHandler> logger,
        IEventRepository eventRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<EventPublishDto>> Handle(GetEventPublishQuery query, CancellationToken cancellationToken)
    {
        var slug = EventSlug.Create(query.Slug);
        var @event = await _eventRepository.GetBySlugAsync(slug, cancellationToken);

        if (@event is null || !@event.IsPublished)
        {
            _logger.LogWarning("Événement avec le slug {Slug} introuvable ou non publié", query.Slug);
            return Result<EventPublishDto>.Failure(Error.NotFound("Event", query.Slug));
        }

        var eventDto = EventDto.FromDomain(@event);
        var categories = @event.Categories.Select(CategoryDto.FromDomain).ToList().AsReadOnly();

        OrganizerProfileDto? organizer = null;
        var organizerId = UserId.FromGuid(@event.OrganizerId);
        var user = await _userRepository.GetByIdAsync(organizerId, cancellationToken);
        if (user is not null)
            organizer = OrganizerProfileDto.FromDomain(user);

        return new EventPublishDto(eventDto, categories, organizer);
    }
}
