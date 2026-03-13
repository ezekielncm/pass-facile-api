namespace Applicattion.User.DTOs;
public sealed record ProfileDto(
    string DisplayName,
    string Bio,
    string LogoUrl,
    string BannerUrl,
    string Slug);