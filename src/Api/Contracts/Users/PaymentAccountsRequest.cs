namespace Api.Contracts.Users
{
    public sealed record PaymentAccountsRequest
    {
        public required string? OrangeMoney { get; init; }
        public required string? MoovMoneyNumber { get; init; }
    }
}
