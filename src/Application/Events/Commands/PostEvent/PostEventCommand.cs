using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Commands.PostEvent
{
    public sealed record PostEventCommand(
        string Name,
        string VenueName,
        string City,
        string Address,
        string? GpsCoordinates,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate) : IRequest<Result<EventDto>>;
}
