using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Queries.GetEventPublish
{
    public sealed class GetEventPublishQueryHandler
        : IRequestHandler<GetEventPublishQuery, Result<EventDto>>
    {
        private readonly ILogger<GetEventPublishQueryHandler> _logger;
        private readonly IEventRepository _eventRepository;
        public GetEventPublishQueryHandler(ILogger<GetEventPublishQueryHandler> logger, IEventRepository eventRepository)
        {
            _logger = logger;
            _eventRepository = eventRepository;
        }
        public async Task<Result<EventDto>> Handle(GetEventPublishQuery cmd, CancellationToken cancellationToken)
        {
            var slug = EventSlug.Create(cmd.Slug);
            var @event = await _eventRepository.GetBySlugAsync(slug, cancellationToken);
            if (@event is null)
            {
                _logger.LogWarning("Event with slug {Slug} not found", cmd.Slug);
                return Result<EventDto>.Failure(Error.NotFound("Event not found", "Event"));
            }
            if (!@event.IsPublished)
            {
                _logger.LogWarning("Event with slug {Slug} is not published", cmd.Slug);
                return Result<EventDto>.Failure(Error.Validation("Event is not published"));
            } 
            _logger.LogInformation("Event with slug {Slug} found and published", cmd.Slug);
            return EventDto.FromDomain(@event);
        }
    }
}
