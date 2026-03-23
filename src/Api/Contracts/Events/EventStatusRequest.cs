namespace Api.Contracts.Events
{
    public sealed record EventStatusRequest
    {
        public required string Status { get; init; }
    }
}
