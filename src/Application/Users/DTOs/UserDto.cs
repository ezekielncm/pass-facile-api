using Domain.Aggregates.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Users.DTOs;
    public sealed record UserDto(
        Guid Id,
        string PhoneNumber,
        string DisplayName,
        string Bio,
        string LogoUrl,
        string BannerUrl,
        string Slug)
{
    public static UserDto FromDomain(Domain.Aggregates.User.User user)
    {
        return new UserDto(
            user.Id.Value,
            user.PhoneNumber.Value,
            user.Profile!.DisplayName,
            user.Profile.Bio,
            user.Profile.LogoUrl,
            user.Profile.BannerUrl,
            user.Profile.Slug);
    }
}