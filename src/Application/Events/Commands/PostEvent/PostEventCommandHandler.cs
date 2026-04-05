using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.Aggregates.Event;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands.PostEvent;

public sealed class PostEventCommandHandler
    : IRequestHandler<PostEventCommand, Result<EventDto>>
{
    private readonly ILogger<PostEventCommandHandler> _logger;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PostEventCommandHandler(
        ILogger<PostEventCommandHandler> logger,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<EventDto>> Handle(PostEventCommand cmd, CancellationToken cancellationToken)
    {
        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        var slug = EventSlug.Create(cmd.Name);
        var venue = Venue.Create(cmd.VenueName, cmd.City, cmd.Address, cmd.GpsCoordinates);
        var salesPeriod = SalesPeriod.Create(cmd.StartDate, cmd.EndDate);

        var @event = Event.Create(
            organizerId, cmd.Name, cmd.Description, slug, venue,
            cmd.StartDate, cmd.EndDate, salesPeriod, []);

        await _eventRepository.AddAsync(@event, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Événement créé avec le slug {Slug}", slug);
        return EventDto.FromDomain(@event);
    }
}
