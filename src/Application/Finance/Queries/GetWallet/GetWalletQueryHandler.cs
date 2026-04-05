using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Finance.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Finance.Queries.GetWallet;

public sealed class GetWalletQueryHandler
    : IRequestHandler<GetWalletQuery, Result<WalletDto>>
{
    private readonly IOrganizerWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetWalletQueryHandler> _logger;

    public GetWalletQueryHandler(
        IOrganizerWalletRepository walletRepository,
        ICurrentUserService currentUserService,
        ILogger<GetWalletQueryHandler> logger)
    {
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<WalletDto>> Handle(GetWalletQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result<WalletDto>.Failure(Error.Validation("Utilisateur non authentifié."));

        var organizerId = Guid.Parse(userId);
        var wallet = await _walletRepository.GetByOrganizerIdAsync(organizerId, cancellationToken);

        if (wallet is null)
            return Result<WalletDto>.Failure(Error.NotFound("Wallet", organizerId));

        var transactions = wallet.Fees
            .Select(TransactionDto.FromDomain)
            .OrderByDescending(t => t.CreatedAt)
            .ToList()
            .AsReadOnly();

        return new WalletDto(
            wallet.Balance.Available.Amount,
            wallet.Balance.Pending.Amount,
            wallet.Currency,
            transactions);
    }
}
