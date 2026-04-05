using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Queries.GetEvents;

public sealed record GetEventsQuery(
    string? Status,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<EventDto>>>;
