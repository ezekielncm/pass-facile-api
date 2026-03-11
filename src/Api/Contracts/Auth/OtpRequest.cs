namespace Api.Contracts.Auth
{
    public sealed record OtpRequest
    {
        public required string PhoneNumber { get; init; }
        public string? Otp { get; init; }
    }
}
