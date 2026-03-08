using Application.Auth.DTOs;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Commands.VerifyOtp
{
    public sealed record VerifyOtpCommand(
        string phoneNumber,
        string otp
        ):IRequest<Result<VerifyOtpDto>>;
}
