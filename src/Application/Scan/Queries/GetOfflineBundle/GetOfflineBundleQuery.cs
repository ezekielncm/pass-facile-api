using Application.Common.Models;
using Application.Scan.DTOs;
using MediatR;

namespace Application.Scan.Queries.GetOfflineBundle;

public sealed record GetOfflineBundleQuery(Guid EventId) : IRequest<Result<OfflineBundleDto>>;
