namespace Application.Events.DTOs;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    int Quota,
    int SoldCount,
    string? Description,
    string FeePolicy,
    bool IsActive)
{
    public static CategoryDto FromDomain(Domain.Aggregates.Event.TicketCategory category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Price.Amount,
            category.Price.Currency,
            category.Quota,
            category.SoldCount,
            category.Description,
            category.FeePolicy.ToString(),
            category.IsActive);
    }
}
