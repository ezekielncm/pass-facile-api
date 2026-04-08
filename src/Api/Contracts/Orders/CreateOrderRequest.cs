namespace Api.Contracts.Orders
{
    /// <summary>Payload pour la création d'une commande.</summary>
    public sealed record CreateOrderRequest
    {
        public required Guid CategoryId { get; init; }
        public required int Quantity { get; init; }
        public required string BuyerPhone { get; init; }
        public string? PromoCode { get; init; }
    }
}
