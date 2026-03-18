namespace Api.Contracts.Events
{
    public sealed record PostEventRequest
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Venue { get; init; }
        public required string Country { get; init; }
        public required string City { get; init; }
        public required string AddressLine1 { get; init; }
        public string? AddressLine2 { get; init; } = null;
        public required string StartDate { get; init; }
        public required string EndDate { get; init; }
        public required string CoverImageUrl { get; init; }
        public required string Capacity { get; init; }
    }
}
