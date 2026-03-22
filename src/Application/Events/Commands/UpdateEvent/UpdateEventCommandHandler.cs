using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.Aggregates.Event;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.UpdateEvent
{
    public sealed class UpdateEventCommandHandler
        :IRequestHandler<UpdateEventCommand,Result<EventDto>>
    {
        private readonly ILogger<UpdateEventCommandHandler> _logger;
        private readonly IEventRepository _eventRepository;

        public UpdateEventCommandHandler(ILogger<UpdateEventCommandHandler> logger, IEventRepository eventRepository)
        {
            _logger = logger;
            _eventRepository = eventRepository;
        }

        public async Task<Result<EventDto>> Handle(UpdateEventCommand cmd, CancellationToken cancellationToken)
        {
            // Implementation for handling the update event command
            var id = Domain.ValueObjects.Identities.EventId.From(cmd.EventId);
            var @event = await _eventRepository.GetByIdAsync(id, cancellationToken);
            var slug = EventSlug.Create(cmd.Name);
            var venue = Venue.Create(cmd.VenueName, cmd.City, cmd.Address, cmd.GpsCoordinates);
            var salesPeriod = SalesPeriod.Create(cmd.SalesStartDate, cmd.SalesEndDate);
            if (@event is null)
            {
                _logger.LogWarning("Event with id {EventId} not found", cmd.EventId);
                return Result<EventDto>.Failure(Error.NotFound("Event not found", "Event"));
            }
            else
            {
                _logger.LogInformation("Event with id {EventId} found", cmd.EventId);
                @event = Event.Create(Guid.Empty, cmd.Name, cmd.Description, slug, venue, cmd.StartDate, cmd.EndDate, salesPeriod, []);
                await _eventRepository.UpdateAsync(@event, cancellationToken);
                return EventDto.FromDomain(@event);
            }
        }
    }
}
