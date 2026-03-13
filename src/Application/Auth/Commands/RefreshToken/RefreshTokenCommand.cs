using Application.Auth.DTOs;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand(
        string RefreshToken) : IRequest<Result<RefreshTokenDto>>;
}
