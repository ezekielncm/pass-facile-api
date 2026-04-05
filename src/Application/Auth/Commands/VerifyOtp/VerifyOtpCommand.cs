using Application.Auth.DTOs;
using Application.Common.Models;
using MediatR;

namespace Application.Auth.Commands.VerifyOtp;

public sealed record VerifyOtpCommand(
    string OtpId,
    string Code,
    string DeviceId) : IRequest<Result<VerifyOtpDto>>;
