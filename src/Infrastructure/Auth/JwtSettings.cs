using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Auth
{
    public sealed class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string SecretKey { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public int ExpirationMinutes { get; init; } = 60;
    }
}
