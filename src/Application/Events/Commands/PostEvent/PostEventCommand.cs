using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Commands.PostEvent;

public sealed record PostEventCommand(
    string Name,
    string Description,
    string VenueName,
    string City,
    string Address,
    string? GpsCoordinates,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? CoverImageUrl,
    int Capacity) : IRequest<Result<EventDto>>;
