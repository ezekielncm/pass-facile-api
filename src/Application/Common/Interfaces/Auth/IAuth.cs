namespace Application.Common.Interfaces.Auth;

public interface IAuth
{
    Task<(bool Success, string? OtpId, DateTimeOffset? ExpiresAt, string? Error)> RequestOtpAsync(string phoneNumber);
    Task<(bool Success, string? AccessToken, string? RefreshToken, string? Error)> VerifyOtpAsync(string phoneNumber, string otp, string deviceId);
    Task<(bool Success, string? AccessToken, string? RefreshToken, string? Error)> RefreshTokenAsync(string refreshToken);
}
