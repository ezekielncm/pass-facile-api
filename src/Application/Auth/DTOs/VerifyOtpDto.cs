namespace Application.Auth.DTOs;

public sealed record VerifyOtpDto(
    string AccessToken,
    string RefreshToken,
    UserDto? User);
