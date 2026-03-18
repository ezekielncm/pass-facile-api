using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.Aggregates.Event;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.PostEvent
{
    //public sealed class PostEventCommandHandler
    //    : IRequestHandler<PostEventCommand, Result<EventDto>>
    //{
    //    private readonly ILogger<PostEventCommandHandler> _logger;
    //    private readonly IEventRepository _eventRepository;
    //    public PostEventCommandHandler(ILogger<PostEventCommandHandler> logger, IEventRepository eventRepository)
    //    {
    //        _logger = logger;
    //        _eventRepository = eventRepository;
    //    }
    //    public async Task<Result<EventDto>> Handle(PostEventCommand cmd, CancellationToken cancellationToken)
    //    {
    //        var slug= EventSlug.Create(cmd.Name);
    //        var venue= Venue.Create(cmd.Venue, cmd.AddressLine1,cmd.AddressLine2, cmd.City,cmd.Country);
    //        var salesPeriod = SalesPeriod.Create(cmd.StartDate,cmd.EndDate);
    //        var eventDate = DateTimeOffset.Parse(cmd.);
    //        var categories = new List<TicketCategory>{};
    //        var @event = Event.Create(slug, venue, salesPeriod, cmd.CoverImageUrl, cmd.Capacity, categories);
    //        //rp = await _eventRepository.AddAsync();
    //    }
    //}
}
