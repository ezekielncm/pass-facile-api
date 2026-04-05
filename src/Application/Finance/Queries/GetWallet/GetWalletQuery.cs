using Application.Common.Models;
using Application.Finance.DTOs;
using MediatR;

namespace Application.Finance.Queries.GetWallet;

public sealed record GetWalletQuery() : IRequest<Result<WalletDto>>;
