using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.DTOs
{
    public sealed record RefreshTokenDto(
        bool? Success,
        string? AccessToken,
        string? RefreshToken,
        string? Error
        )
    {
    }
}
