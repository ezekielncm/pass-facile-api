using Application.Common.Models;
using Application.Users.DTOs;
using MediatR;

namespace Application.Users.Commands.UpdatePaymentAccounts;

public sealed record UpdatePaymentAccountsCommand(
    string? OrangeMoneyNumber,
    string? MoovMoneyNumber) : IRequest<Result<PaymentAccountsDto>>;
