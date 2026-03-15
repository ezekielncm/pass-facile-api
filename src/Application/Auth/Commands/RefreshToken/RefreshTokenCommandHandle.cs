using Application.Auth.DTOs;
using Application.Common.Interfaces.Auth;
using Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandle
        :IRequestHandler<RefreshTokenCommand,Result<RefreshTokenDto>>
    {
        private readonly IAuth _auth;
        private readonly ILogger<RefreshTokenCommandHandle> _logger;
        public RefreshTokenCommandHandle(IAuth auth,ILogger<RefreshTokenCommandHandle> logger)
        {
            _auth = auth;
            _logger = logger;
        }
        public async Task<Result<RefreshTokenDto>> Handle(RefreshTokenCommand command,CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
