using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces.Auth
{
    public interface IAuth
    {
        //Task<(bool Success, string? Token, string? Error)> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
        //Task<(bool Success, string? Error)> RegisterAsync(string email, string password, string firstName, string lastName, string role, CancellationToken cancellationToken = default);
        //Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        //Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
        //Task<string> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> RequestOtpAsync(string phoneNumber);
        Task<(bool Success, string? Token, string? Error)> VerifyOtpAsync(string phoneNumber, string otp);
    }
}
