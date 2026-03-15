using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.Aggregates.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.PostEvent
{
    public sealed class PostEventCommandHandler
        : IRequestHandler<PostEventCommand, Result<EventDto>>
    {
        private readonly ILogger<PostEventCommandHandler> _logger;
        private readonly IEventRepository _eventRepository;
        public PostEventCommandHandler(ILogger<PostEventCommandHandler> logger, IEventRepository eventRepository)
        {
            _logger = logger;
            _eventRepository = eventRepository;
        }
        public async Task<Result<EventDto>> Handle(PostEventCommand cmd, CancellationToken cancellationToken)
        {

            rp = await _eventRepository.AddAsync();
        }
    }
}
