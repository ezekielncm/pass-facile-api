using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.Queries.GetEventPublish
{
    public sealed record GetEventPublishQuery(string Slug)
        :IRequest<Result<EventDto>>;
}
