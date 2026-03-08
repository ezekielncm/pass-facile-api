using Application.Auth.DTOs;
using Application.Common.Interfaces.Auth;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Commands.RequestOtp
{
    public sealed class RequestOtpCommandHandle
        : IRequestHandler<RequestOtpCommand, Result<RequestOtpDto>>
    {
        IAuth _auth;
        public RequestOtpCommandHandle(IAuth auth) => _auth = auth;
        public async Task<Result<RequestOtpDto>> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
        {
            var result = await _auth.RequestOtpAsync(request.PhoneNumber);
            RequestOtpDto otpDto = new(result.Success, result.Error);
            return otpDto;
        }
    }
}
