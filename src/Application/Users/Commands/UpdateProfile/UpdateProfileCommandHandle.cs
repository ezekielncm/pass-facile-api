using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Users.DTOs;
using Domain.Aggregates.User;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandle
    : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly IAuth _auth;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateProfileCommandHandle(IUserRepository userRepository, ILogger<UpdateProfileCommandHandle> logger, ICurrentUserService currentUserService, IAuth auth, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _logger = logger;
        _currentUserService = currentUserService;
        _auth = auth;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<UserDto>> Handle(UpdateProfileCommand cmd, CancellationToken cancellationToken)
    {
        var phone = _currentUserService.PhoneNumber;
        //var id = _currentUserService.UserId;
        if (phone is null)
        {
            return null;
        }
        //var userId = UserId.FromGuid(Guid.Parse(id));
        var phoneNumber = new PhoneNumber(phone);
        var profile = new UserProfile(cmd.DisplayName, cmd.Bio, cmd.LogoUrl, cmd.BannerUrl, cmd.Slug);
        var user = User.Register(phoneNumber, profile);
        await _userRepository.AddAsync(user,CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var result = UserDto.FromDomain(user);
        return result;
    }
}