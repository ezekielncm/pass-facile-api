using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.DTOs
{
    public sealed record RequestOtpDto(
        bool Success, 
        string? Error
        );
}
