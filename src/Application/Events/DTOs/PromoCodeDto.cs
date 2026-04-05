namespace Application.Events.DTOs;

public sealed record PromoCodeDto(
    Guid Id,
    string Code,
    string DiscountType,
    decimal Value,
    int MaxUses,
    int UsedCount,
    DateTimeOffset? ExpiresAt,
    bool IsActive)
{
    public static PromoCodeDto FromDomain(Domain.Aggregates.Event.PromoCode promoCode)
    {
        return new PromoCodeDto(
            promoCode.Id,
            promoCode.Code,
            promoCode.DiscountType.ToString(),
            promoCode.Value,
            promoCode.MaxUses,
            promoCode.UsedCount,
            promoCode.ExpiresAt,
            promoCode.IsActive);
    }
}
