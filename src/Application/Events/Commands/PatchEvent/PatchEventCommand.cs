using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Commands.PatchEvent
{
    public sealed record PatchEventCommand(Guid Id, string Status)
        : IRequest<Result<EventDto>>;
}
