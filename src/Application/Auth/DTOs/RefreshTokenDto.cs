namespace Application.Auth.DTOs;

public sealed record RefreshTokenDto(
    string AccessToken,
    string RefreshToken);
