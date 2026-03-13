namespace Api.Contracts.Auth
{
    public sealed record SendOtpRequest
    {
        public required string PhoneNumber { get; init; }
    }
}
