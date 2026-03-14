using Application.Common.Models;
using Applicattion.User.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.User.Commands.UpdateProfile
{
    public sealed record UpdateProfileCommand(
        string DisplayName,
        string Bio,
        string LogoUrl,
        string BannerUrl,
        string Slug) : IRequest<Result<ProfileDto>>;
}
