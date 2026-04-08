namespace Api.Contracts.Scan
{
    /// <summary>Payload pour la synchronisation de scans offline.</summary>
    public sealed record SyncScansRequest
    {
        public required IReadOnlyCollection<OfflineScanItem> Scans { get; init; }
    }

    /// <summary>Élément de scan offline.</summary>
    public sealed record OfflineScanItem
    {
        public required string QrPayload { get; init; }
        public required DateTimeOffset ScannedAt { get; init; }
        public required string DeviceId { get; init; }
    }
}
