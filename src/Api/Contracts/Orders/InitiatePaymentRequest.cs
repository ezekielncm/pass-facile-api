namespace Api.Contracts.Orders
{
    /// <summary>Payload pour l'initiation d'un paiement.</summary>
    public sealed record InitiatePaymentRequest
    {
        public required string PaymentPhone { get; init; }
        public required string Provider { get; init; }
    }
}
