using Application.Common.Models;
using Application.Scan.DTOs;
using MediatR;

namespace Application.Scan.Commands.ScanTicket;

public sealed record ScanTicketCommand(
    string QrPayload,
    string DeviceId,
    DateTimeOffset ScannedAt) : IRequest<Result<ScanResultDto>>;
