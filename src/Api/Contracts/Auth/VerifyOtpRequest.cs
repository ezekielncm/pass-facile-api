namespace Api.Contracts.Auth;
public sealed record VerifyOtpRequest
{
    public required string PhoneNumber { get; init; }
    public required string Otp { get; init; }
}