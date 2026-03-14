using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.DTOs
{
    public sealed record VerifyOtpDto(
        bool Success,
        string? AccessToken,
        //string? RefreshToken,
        //User? User,
        string? Error
        );
}
