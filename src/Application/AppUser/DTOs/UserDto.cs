using System;
using System.Collections.Generic;
using System.Text;

namespace Application.User.DTOs;
    public sealed record UserDto(
        Guid Id,
        string PhoneNumber,
        string DisplayName,
        string Bio,
        string LogoUrl,
        string BannerUrl,
        string Slug)
    {
       
    }
