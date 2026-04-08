namespace Application.Users.DTOs;

public sealed record PaymentAccountsDto(
    string? OrangeMoneyNumber,
    string? MoovMoneyNumber);
