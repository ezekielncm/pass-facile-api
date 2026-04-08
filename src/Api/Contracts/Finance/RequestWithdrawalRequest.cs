namespace Api.Contracts.Finance
{
    /// <summary>Payload pour une demande de retrait.</summary>
    public sealed record RequestWithdrawalRequest
    {
        public required decimal Amount { get; init; }
        public required Guid AccountId { get; init; }
    }
}
