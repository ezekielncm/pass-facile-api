namespace Application.Auth.DTOs;

public sealed record RequestOtpDto(
    string OtpId,
    DateTimeOffset ExpiresAt);
