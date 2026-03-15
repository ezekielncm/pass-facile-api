using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Users.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Users.Commands.UpdateProfile;
//public sealed class UpdateProfileCommandHandle
//    : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
//{
//    private readonly IUserRepository _userRepository;
//    private readonly ILogger _logger;
//    public UpdateProfileCommandHandle(IUserRepository userRepository, ILogger<UpdateProfileCommandHandle> logger)
//    {
//        _userRepository = userRepository;
//        _logger = logger;
//    }
//    public async Task<Result<UserDto>> Handle(UpdateProfileCommand cmd, CancellationToken cancellationToken)
//    {

//        //var result=_auth.UpdateProfile(cmd);
//        //UserDto user = new UserDto();
//        var rp = await _userRepository.UpdateProfile();                                                 
//        var result = UserDto.FromDomain(rp.User!);
//        return result;
//    }
//}