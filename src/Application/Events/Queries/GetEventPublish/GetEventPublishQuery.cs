using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Queries.GetEventPublish;

public sealed record GetEventPublishQuery(string Slug)
    : IRequest<Result<EventPublishDto>>;
