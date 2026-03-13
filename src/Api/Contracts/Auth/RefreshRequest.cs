namespace Api.Contracts.Auth
{
    public sealed record RefreshRequest
    {
        public required string RefreshToken { get; init; }
    }
}
