using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.UpdateEvent
{
    public sealed record UpdateEventCommand(
        Guid EventId,
        string Name,
        string Description,
        string VenueName,
        string City,
        string Address,
        string? GpsCoordinates,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate) : IRequest<Result<EventDto>>;
}
