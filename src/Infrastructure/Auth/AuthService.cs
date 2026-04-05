using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Auth
{
    public class AuthService : IAuth
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOtpService _otpService;
        // private readonly ISmsService _smsService;
        private readonly JwtTokenGenerator _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IkkodiClient _client;

        public AuthService(
            UserManager<AppUser> userManager,
            IOtpService otpService,
            //ISmsService smsService,
            JwtTokenGenerator jwtService,
            IkkodiClient client,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _otpService = otpService;
            // _smsService = smsService;
            _jwtService = jwtService;
            _logger = logger;
            _client = client;
        }

        // Étape 1 : demande d'OTP
        public async Task<(bool Success, string? OtpId, DateTimeOffset? ExpiresAt, string? Error)> RequestOtpAsync(string phoneNumber)
        {
            // Créer le user s'il n'existe pas (first login = auto-register)
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);


                if (user is null)
                {
                    _logger.LogInformation("new user{phoneNumber}", phoneNumber);

                    user = new AppUser
                    {
                        UserName = phoneNumber,
                        PhoneNumber = phoneNumber
                    };
                    try
                    {
                        var result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                            return (false, null, null, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error creating user with phone number {PhoneNumber}", phoneNumber);
                        return (false, null, null, "Error creating user.");
                    }

                    //await _userManager.AddToRoleAsync(user,"User"); // rôle par défaut
                }


                if (!user.IsActive)
                    return (false, null, null, "Account disabled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing user with phone number {PhoneNumber}", phoneNumber);
                return (false, null, null, "Error accessing user data.");
            }
            var otp = await _otpService.GenerateAndStoreOtpAsync(phoneNumber);

            // En production → SMS. En dev → log.
            //await _smsService.SendAsync(phoneNumber, $"Your verification code: {otp}. Valid 5 minutes.");
            try
            {
                //await _client.SendSmsAsync($"226{phoneNumber}", $"Your verification code: {otp}. Valid 5 minutes.");
            }catch(Exception e)
            {
                _logger.LogError(e, "Error sending OTP SMS to {PhoneNumber}", phoneNumber);
            }

            var otpId = Guid.NewGuid().ToString();
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);
            return (true, otpId, expiresAt, null);
        }

        // Étape 2 : vérification OTP → JWT
        public async Task<(bool Success, string? AccessToken, string? RefreshToken, string? Error)> VerifyOtpAsync(string phoneNumber, string otp, string deviceId)
        {
            var valid = await _otpService.ValidateOtpAsync(phoneNumber, otp);
            if (!valid)
                return (false, null, null, "Invalid or expired OTP.");

            var user = await _userManager.Users  
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user is null || !user.IsActive)
                return (false, null, null, "User not found or disabled.");

            // Marquer le numéro comme confirmé
            if (!user.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtService.GenerateToken(user, roles);
            var refreshToken = Guid.NewGuid().ToString();

            return (true, accessToken, refreshToken, null);
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, string? Error)> RefreshTokenAsync(string refreshToken)
        {
            // TODO: Implement proper refresh token validation
            return (false, null, null, "Refresh token support not yet implemented.");
        }
    }
}
