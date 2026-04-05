using Application.Auth.DTOs;
using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Domain.ValueObjects;
using MediatR;

namespace Application.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandHandler
    : IRequestHandler<VerifyOtpCommand, Result<VerifyOtpDto>>
{
    private readonly IAuth _auth;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyOtpCommandHandler(IAuth auth, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _auth = auth;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VerifyOtpDto>> Handle(VerifyOtpCommand cmd, CancellationToken cancellationToken)
    {
        var result = await _auth.VerifyOtpAsync(cmd.OtpId, cmd.Code, cmd.DeviceId);

        if (!result.Success)
            return Result<VerifyOtpDto>.Failure(Error.Validation(result.Error ?? "OTP invalide."));

        return new VerifyOtpDto(result.AccessToken!, result.RefreshToken!, null);
    }
}
