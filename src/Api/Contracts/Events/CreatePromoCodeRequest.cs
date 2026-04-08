namespace Api.Contracts.Events
{
    /// <summary>Payload pour la création d'un code promo.</summary>
    public sealed record CreatePromoCodeRequest
    {
        public required string Code { get; init; }
        public required string DiscountType { get; init; }
        public required decimal Value { get; init; }
        public required int MaxUses { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
    }
}
