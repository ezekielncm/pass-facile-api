namespace Application.Scan.DTOs;

public sealed record ScanResultDto(
    string Result,
    TicketInfoDto? Ticket,
    Guid ScanEventId,
    DateTimeOffset ScannedAt);

public sealed record TicketInfoDto(
    string Reference,
    string Category,
    string Status);
