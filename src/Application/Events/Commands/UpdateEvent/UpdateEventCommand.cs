using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Commands.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string? Name,
    string? Description,
    string? VenueName,
    string? City,
    string? Address,
    string? GpsCoordinates,
    string? CoverImageUrl,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    int? Capacity) : IRequest<Result<EventDto>>;
