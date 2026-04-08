namespace Api.Contracts.Scan
{
    /// <summary>Payload pour le scan d'un ticket.</summary>
    public sealed record ScanTicketRequest
    {
        public required string QrPayload { get; init; }
        public required string DeviceId { get; init; }
        public required DateTimeOffset ScannedAt { get; init; }
    }
}
