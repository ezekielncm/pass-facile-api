using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.DuplicateEvent
{
    public sealed record DuplicateEventCommand(Guid Id, DateTimeOffset NewStartDate, DateTimeOffset NewEndDate) : IRequest<Result<EventDto>>;
}
