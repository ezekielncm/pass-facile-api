namespace Api.Contracts.Users
{
    public sealed record UpdateProfileRequest
    {
        public required string DisplayName { get; init; }
        public required string Bio { get; init; }
        public required string LogoUrl { get; init; }
        public required string BannerUrl { get; init; }
        public required string Slug { get; init; }
    }
}
