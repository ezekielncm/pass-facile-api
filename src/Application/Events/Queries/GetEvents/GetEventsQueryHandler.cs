using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Queries.GetEvents
{
    public sealed class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, Result<PagedResult<EventDto>>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetEventsQueryHandler> _logger;
        public GetEventsQueryHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork, ILogger<GetEventsQueryHandler> logger)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result<PagedResult<EventDto>>> Handle(GetEventsQuery cmd, CancellationToken cancellationToken)
        {
            //var id = Domain.ValueObjects.Identities.UserId.FromGuid(cmd.Id);
            var events = await _eventRepository.GetByOrganizerIdAsync(cmd.Id, cancellationToken);
            if (events is null || events.Count() == 0)
            {
                _logger.LogWarning("No events found for organizer with id {OrganizerId}", cmd.Id);
                return Result<PagedResult<EventDto>>.Failure(Error.NotFound("NotFound", $"No events found for organizer with id {cmd.Id}"));
            }
            var eventDtos = events.Select(EventDto.FromDomain).ToList();
            return new PagedResult<EventDto>(eventDtos, eventDtos.Count, 1, eventDtos.Count);
        }
    }
}
