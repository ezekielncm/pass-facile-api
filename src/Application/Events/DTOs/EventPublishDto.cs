namespace Application.Events.DTOs;

public sealed record EventPublishDto(
    EventDto Event,
    IReadOnlyCollection<CategoryDto> Categories,
    OrganizerProfileDto? OrganizerProfile);

public sealed record OrganizerProfileDto(
    Guid Id,
    string? DisplayName,
    string? Bio,
    string? LogoUrl,
    string? BannerUrl,
    string? Slug)
{
    public static OrganizerProfileDto FromDomain(Domain.Aggregates.User.User user)
    {
        return new OrganizerProfileDto(
            user.Id.Value,
            user.Profile?.DisplayName,
            user.Profile?.Bio,
            user.Profile?.LogoUrl,
            user.Profile?.BannerUrl,
            user.Profile?.Slug);
    }
}
