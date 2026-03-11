namespace Api.Contracts.Events
{
    public sealed record CreateEventRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Venue { get; init; }
        public required string StartDate { get; init; }
        public required string EndDate { get; init; }
        public required string CoverImageUrl { get; init; }
        public required string Capacity { get; init; }
    }
}
