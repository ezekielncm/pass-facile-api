namespace Application.Events.DTOs;

public sealed record EventDto(
    Guid Id,
    string Name,
    string? Description,
    string Slug,
    string Venue,
    string? CoverImageUrl,
    string StartDate,
    string EndDate,
    string Capacity,
    string Status,
    Guid OrganizerId,
    IReadOnlyCollection<CategoryDto> Categories)
{
    public static EventDto FromDomain(Domain.Aggregates.Event.Event @event)
    {
        var categories = @event.Categories
            .Select(CategoryDto.FromDomain)
            .ToList()
            .AsReadOnly();

        return new EventDto(
            @event.Id.Value,
            @event.Name,
            @event.Description,
            @event.Slug.ToString(),
            @event.Venue.ToString(),
            @event.CoverImageUrl,
            @event.StartDate.ToString("o"),
            @event.EndDate.ToString("o"),
            @event.Capacity.ToString(),
            @event.Status.ToString(),
            @event.OrganizerId,
            categories);
    }
}
