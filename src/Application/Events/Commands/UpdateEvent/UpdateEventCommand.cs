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
        string Country,
        string City,
        string AddressLine1,
        string? AddressLine2,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset EventDate) : IRequest<Result<EventDto>>;
}
