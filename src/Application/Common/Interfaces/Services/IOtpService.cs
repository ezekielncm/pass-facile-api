using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(string phoneNumber);
        Task<bool> ValidateOtpAsync(string phoneNumber, string otp);
    }
}
