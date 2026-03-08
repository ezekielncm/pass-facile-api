using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Services;
using Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Auth
{
    public class AuthService : IAuth
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly ISmsService _smsService;
        private readonly JwtTokenService _jwtService;

        public AuthService(
            UserManager<AppUser> userManager,
            IOtpService otpService,
            ISmsService smsService,
            JwtTokenService jwtService)
        {
            _userManager = userManager;
            _otpService = otpService;
            _smsService = smsService;
            _jwtService = jwtService;
        }

        // Étape 1 : demande d'OTP
        public async Task<(bool, string?)> RequestOtpAsync(string phoneNumber)
        {
            // Créer le user s'il n'existe pas (first login = auto-register)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user is null)
            {
                user = new AppUser
                {
                    UserName = phoneNumber,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = false,
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user, "Doctor"); // rôle par défaut
            }

            if (!user.IsActive)
                return (false, "Account disabled.");

            var otp = await _otpService.GenerateAndStoreOtpAsync(phoneNumber);

            // En production → SMS. En dev → log.
            await _smsService.SendAsync(phoneNumber, $"Your verification code: {otp}. Valid 5 minutes.");

            return (true, null);
        }

        // Étape 2 : vérification OTP → JWT
        public async Task<(bool, string?, string?)> VerifyOtpAsync(string phoneNumber, string otp)
        {
            var valid = await _otpService.ValidateOtpAsync(phoneNumber, otp);
            if (!valid)
                return (false, null, "Invalid or expired OTP.");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user is null || !user.IsActive)
                return (false, null, "User not found or disabled.");

            // Marquer le numéro comme confirmé
            if (!user.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            return (true, token, null);
        }
    }
}
