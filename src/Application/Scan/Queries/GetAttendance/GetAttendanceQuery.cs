using Application.Common.Models;
using Application.Scan.DTOs;
using MediatR;

namespace Application.Scan.Queries.GetAttendance;

public sealed record GetAttendanceQuery(Guid EventId) : IRequest<Result<AttendanceDto>>;
