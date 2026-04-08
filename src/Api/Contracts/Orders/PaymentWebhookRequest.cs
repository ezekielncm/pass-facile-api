namespace Api.Contracts.Orders
{
    /// <summary>Payload pour le callback de paiement.</summary>
    public sealed record PaymentWebhookRequest
    {
        public required string TransactionId { get; init; }
        public required string Status { get; init; }
        public string? FailureReason { get; init; }
        public required string Signature { get; init; }
    }
}
