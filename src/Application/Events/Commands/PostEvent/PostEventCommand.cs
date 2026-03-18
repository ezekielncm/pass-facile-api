using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.PostEvent
{
    public sealed record PostEventCommand(
        string Name,
        string Description,
        string Venue,
        string Country,
        string City,
        string AddressLine1,
        string? AddressLine2,
        DateTime StartDate,
        DateTime EndDate,
        string CoverImageUrl,
        int Capacity):IRequest<Result<EventDto>>
    {
    }
}
