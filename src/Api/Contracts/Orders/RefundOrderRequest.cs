namespace Api.Contracts.Orders
{
    /// <summary>Payload pour le remboursement d'une commande.</summary>
    public sealed record RefundOrderRequest
    {
        public decimal? Amount { get; init; }
        public required string Reason { get; init; }
    }
}
