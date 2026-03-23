using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.PatchEvent
{
    public sealed class PatchEventCommandHandler
        : IRequestHandler<PatchEventCommand, Result<EventDto>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PatchEventCommandHandler> _logger;

        public PatchEventCommandHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork, ILogger<PatchEventCommandHandler> logger)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result<EventDto>> Handle(PatchEventCommand cmd, CancellationToken cancellationToken)
        {
            var id = Domain.ValueObjects.Identities.EventId.From(cmd.Id);
            var eventToUpdate = await _eventRepository.GetByIdAsync(id, cancellationToken);
            if (eventToUpdate is null)
            {
                _logger.LogWarning("Event with id {EventId} not found", id);
                return Result<EventDto>.Failure(Error.NotFound("NotFound", $"Event with id {id} not found"));
            }
            if (cmd.Status == "Publish")
                eventToUpdate.Publish();
            if (cmd.Status == "Draft")
                eventToUpdate.Unpublish();
            await _eventRepository.UpdateAsync(eventToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EventDto.FromDomain(eventToUpdate);
        }
    }
}
