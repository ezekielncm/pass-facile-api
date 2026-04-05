using Application.Auth.DTOs;
using Application.Common.Interfaces.Auth;
using Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenDto>>
{
    private readonly IAuth _auth;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(IAuth auth, ILogger<RefreshTokenCommandHandler> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenDto>> Handle(RefreshTokenCommand cmd, CancellationToken cancellationToken)
    {
        var result = await _auth.RefreshTokenAsync(cmd.RefreshToken);

        if (!result.Success)
        {
            _logger.LogWarning("Échec du rafraîchissement du token.");
            return Result<RefreshTokenDto>.Failure(Error.Validation(result.Error ?? "Refresh token invalide ou expiré."));
        }

        return new RefreshTokenDto(result.AccessToken!, result.RefreshToken!);
    }
}
