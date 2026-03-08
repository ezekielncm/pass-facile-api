using Application.Auth.DTOs;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Commands.RequestOtp
{
    public sealed record RequestOtpCommand(
        string PhoneNumber
        ) : IRequest<Result<RequestOtpDto>>;
}
