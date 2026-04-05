using Application.Common.Models;
using Application.Scan.DTOs;
using MediatR;

namespace Application.Scan.Commands.SyncScans;

public sealed record SyncScansCommand(
    IReadOnlyCollection<OfflineScanDto> Scans) : IRequest<Result<SyncResultDto>>;

public sealed record OfflineScanDto(
    string QrPayload,
    DateTimeOffset ScannedAt,
    string DeviceId);

public sealed record SyncResultDto(
    int Synced,
    IReadOnlyCollection<SyncConflictDto> Conflicts);

public sealed record SyncConflictDto(
    string QrPayload,
    string Reason);
