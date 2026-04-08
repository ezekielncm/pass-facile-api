namespace Application.Auth.DTOs;

public sealed record UserDto(
    Guid Id,
    string PhoneNumber,
    bool IsVerified,
    string? DisplayName)
{
    public static UserDto FromDomain(Domain.Aggregates.User.User user)
    {
        return new UserDto(
            user.Id.Value,
            user.PhoneNumber.Value,
            user.IsVerified,
            user.DisplayName);
    }
}
