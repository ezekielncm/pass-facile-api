namespace Application.Scan.DTOs;

public sealed record OfflineBundleDto(
    IReadOnlyCollection<OfflineTicketDto> Tickets,
    DateTimeOffset GeneratedAt,
    string Signature);

public sealed record OfflineTicketDto(
    string QrPayload,
    string TicketRef,
    string Category);
