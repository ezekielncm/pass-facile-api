namespace Api.Contracts.Events
{
    public sealed record DuplicateEventRequest
    {
        public required string newStartDate { get; init; }
        public required string newEndDate { get; init; }
    }
}
