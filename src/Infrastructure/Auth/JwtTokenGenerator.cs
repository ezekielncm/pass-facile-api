using Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Twilio.Base;

namespace Infrastructure.Auth
{
    public class JwtTokenGenerator
    {
        private readonly JwtSettings _settings;
        public JwtTokenGenerator(IOptions<JwtSettings> s) => _settings = s.Value;

        public string GenerateToken(AppUser user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,         user.Id),
            new(JwtRegisteredClaimNames.Jti,         Guid.NewGuid().ToString()),
            new(ClaimTypes.MobilePhone,              user.PhoneNumber!),
            new("fullName", $"{user.FirstName} {user.LastName}"),
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
