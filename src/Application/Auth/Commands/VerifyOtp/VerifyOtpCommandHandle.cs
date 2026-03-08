using Application.Auth.DTOs;
using Application.Common.Interfaces.Auth;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Commands.VerifyOtp
{
    public sealed class VerifyOtpCommandHandle
        :IRequestHandler<VerifyOtpCommand, Result<VerifyOtpDto>>
    {
        private readonly IAuth _auth;
        public VerifyOtpCommandHandle(IAuth auth) => _auth = auth;
        public async Task<Result<VerifyOtpDto>> Handle(VerifyOtpCommand cmd,CancellationToken cancellationToken)
        {
            var result = await _auth.VerifyOtpAsync(cmd.phoneNumber, cmd.otp);
            VerifyOtpDto response= new(result.Success, result.Token, result.Error);
            return response;
        }
    }
}
