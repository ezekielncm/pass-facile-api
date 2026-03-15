using Application.Common.Models;
using Application.Users.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Users.Commands.UpdateProfile
{
    public sealed record UpdateProfileCommand(
        string DisplayName,
        string Bio,
        string LogoUrl,
        string BannerUrl,
        string Slug) : IRequest<Result<UserDto>>;
}
