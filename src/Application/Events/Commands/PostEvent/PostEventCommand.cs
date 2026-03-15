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
        string StartDate,
        string EndDate,
        string CoverImageUrl,
        string Capacity):IRequest<Result<EventDto>>
    {
    }
}
