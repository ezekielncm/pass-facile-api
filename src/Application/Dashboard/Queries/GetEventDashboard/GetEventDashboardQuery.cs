using Application.Common.Models;
using Application.Dashboard.DTOs;
using MediatR;

namespace Application.Dashboard.Queries.GetEventDashboard;

public sealed record GetEventDashboardQuery(Guid EventId) : IRequest<Result<DashboardDto>>;
