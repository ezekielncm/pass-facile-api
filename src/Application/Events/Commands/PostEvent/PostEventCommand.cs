using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Commands.PostEvent
{
    public sealed record PostEventCommand(
        string Name,
        string VenueName,
        string Country,
        string City,
        string AddressLine1,
        string? AddressLine2,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset EventDate) : IRequest<Result<EventDto>>;
}
