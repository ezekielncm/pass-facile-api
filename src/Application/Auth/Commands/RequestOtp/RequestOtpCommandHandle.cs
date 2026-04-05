using Application.Auth.DTOs;
using Application.Common.Interfaces.Auth;
using Application.Common.Models;
using MediatR;

namespace Application.Auth.Commands.RequestOtp;

public sealed class RequestOtpCommandHandler
    : IRequestHandler<RequestOtpCommand, Result<RequestOtpDto>>
{
    private readonly IAuth _auth;

    public RequestOtpCommandHandler(IAuth auth) => _auth = auth;

    public async Task<Result<RequestOtpDto>> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var result = await _auth.RequestOtpAsync(request.PhoneNumber);

        if (!result.Success)
            return Result<RequestOtpDto>.Failure(Error.Validation(result.Error ?? "Échec de l'envoi de l'OTP."));

        return new RequestOtpDto(result.OtpId!, result.ExpiresAt!.Value);
    }
}
