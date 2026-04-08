using Application.Common.Interfaces.Messaging;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.DuplicateEvent
{
    public sealed class DuplicateEventCommandHandler
        : IRequestHandler<DuplicateEventCommand, Result<EventDto>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<DuplicateEventCommandHandler> _logger;
        private readonly IUnitOfWork _unitOflWork;
        public DuplicateEventCommandHandler(IEventRepository eventRepository, ILogger<DuplicateEventCommandHandler> logger, IUnitOfWork unitOflWork)
        {
            _eventRepository = eventRepository;
            _logger = logger;
            _unitOflWork = unitOflWork;
        }
        public async Task<Result<EventDto>> Handle(DuplicateEventCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                var id = Domain.ValueObjects.Identities.EventId.From(cmd.Id);
                var eventToDuplicate = await _eventRepository.GetByIdAsync(id, cancellationToken);
                if (eventToDuplicate != null)
                {
                    var duplicatedEvent = eventToDuplicate.Duplicate(eventToDuplicate.Slug, eventToDuplicate.SalesPeriod, cmd.NewStartDate, cmd.NewEndDate);
                    await _eventRepository.AddAsync(duplicatedEvent, cancellationToken);
                    await _unitOflWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Event with id {EventId} duplicated successfully", id);
                    return EventDto.FromDomain(duplicatedEvent);
                }
                
                _logger.LogWarning("Event with id {EventId} not found for duplication", id);
                return Result<EventDto>.Failure(new Error("NotFound", $"Event with id {cmd.Id} not found."));
                

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while duplicating the event with id {EventId}.", cmd.Id);
                return Result<EventDto>.Failure(new Error("ServerError", "An error occurred while processing your request. Please try again later."));
            }
        }
    }
}
