namespace Api.Contracts.Users
{
    public sealed record paymentAccountsRequest
    {
        public required string? OrangeMoney { get; init; }
        public required string? MoovMoneyNumber { get; init; }
    }
}
