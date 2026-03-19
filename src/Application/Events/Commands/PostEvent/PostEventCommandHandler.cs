using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.Aggregates.Event;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands.PostEvent
{
    public sealed class PostEventCommandHandler
        : IRequestHandler<PostEventCommand, Result<EventDto>>
    {
        private readonly ILogger<PostEventCommandHandler> _logger;
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PostEventCommandHandler(
            ILogger<PostEventCommandHandler> logger,
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<EventDto>> Handle(PostEventCommand cmd, CancellationToken cancellationToken)
        {
            var slug = EventSlug.Create(cmd.Name);
            var venue = Venue.Create(cmd.VenueName, cmd.AddressLine1, cmd.AddressLine2, cmd.City, cmd.Country);
            var salesPeriod = SalesPeriod.Create(cmd.SalesStartDate, cmd.SalesEndDate);

            var @event = Event.Create(cmd.Name,$"{cmd.Name}",slug, venue, salesPeriod, cmd.EventDate, []);

            await _eventRepository.AddAsync(@event, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EventDto.FromDomain(@event);
        }
    }
}
