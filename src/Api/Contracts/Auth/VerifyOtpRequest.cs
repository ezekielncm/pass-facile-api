namespace Api.Contracts.Auth;
public sealed record VerifyOtpRequest
{
    public required string OtpId { get; init; }
    public required string Code { get; init; }
    public required string DeviceId { get; init; }
}