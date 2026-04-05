using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Users.DTOs;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler
    : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        ILogger<UpdateProfileCommandHandler> logger,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _logger = logger;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand cmd, CancellationToken cancellationToken)
    {
        var phone = _currentUserService.PhoneNumber;
        if (phone is null)
            return Result<UserDto>.Failure(Error.Validation("Numéro de téléphone introuvable."));

        var phoneNumber = new PhoneNumber(phone);
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);

        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("User", phone));

        var profile = new UserProfile(cmd.DisplayName, cmd.Bio, cmd.LogoUrl, cmd.BannerUrl, cmd.Slug);
        user.UpdateProfile(profile);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.FromDomain(user);
    }
}