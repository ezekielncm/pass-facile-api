using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Applicattion.User.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.User.Commands.UpdateProfile;
public sealed class UpdateProfileCommandHandle
    : IRequestHandler<UpdateProfileCommand, Result<ProfileDto>>
{
    private readonly IAuth _auth;
    private readonly ILogger _logger;
    public UpdateProfileCommandHandle(IAuth auth, ILogger<UpdateProfileCommandHandle> logger)
    {
        _auth = auth;
        _logger = logger;
    }
    public async Task<Result<ProfileDto>> Handle(UpdateProfileCommand cmd, CancellationToken cancellationToken)
    {
        //var result=_auth.UpdateProfile(cmd);
        ProfileDto profile = new ProfileDto(false,"");
        return profile;
    }
}