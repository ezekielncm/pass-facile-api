namespace Api.Contracts.Notifications
{
    /// <summary>Payload pour l'envoi d'un rappel aux participants.</summary>
    public sealed record SendReminderRequest
    {
        public required Guid EventId { get; init; }
        public string? Message { get; init; }
        public DateTimeOffset? ScheduledAt { get; init; }
    }
}
