using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Queries.GetEvents
{
    public sealed record GetEventsQuery(Guid Id) : IRequest<Result<PagedResult<EventDto>>>;
}
