namespace Api.Contracts.Auth
{
    public sealed record RefreshRequest
    {
        public required string refreshToken { get; init; }
    }
}
