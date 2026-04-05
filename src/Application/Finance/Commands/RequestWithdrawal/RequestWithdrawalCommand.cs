using Application.Common.Models;
using Application.Finance.DTOs;
using MediatR;

namespace Application.Finance.Commands.RequestWithdrawal;

public sealed record RequestWithdrawalCommand(
    decimal Amount,
    Guid AccountId) : IRequest<Result<WithdrawalDto>>;
