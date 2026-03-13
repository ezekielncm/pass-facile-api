using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Applicattion.User.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.User.Commands.UpdateProfile;
public sealed class UpdateProfileCommandHanle
    : IRequestHandler<UpdateProfileCommand, Result<ProfileDto>>
{
    private readonly IUserRepository _repository;
    private readonly ILogger _logger;
    public UpdateProfileCommandHanle(IUserRepository repository, ILogger<UpdateProfileCommandHanle> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<Result<ProfileDto>> Handle(UpdateProfileCommand cmd, CancellationToken cancellationToken)
    {
        return null;
    }
}