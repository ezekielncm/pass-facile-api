using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Finance.DTOs;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Finance.Commands.RequestWithdrawal;

public sealed class RequestWithdrawalCommandHandler
    : IRequestHandler<RequestWithdrawalCommand, Result<WithdrawalDto>>
{
    private readonly IOrganizerWalletRepository _walletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestWithdrawalCommandHandler> _logger;

    public RequestWithdrawalCommandHandler(
        IOrganizerWalletRepository walletRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<RequestWithdrawalCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WithdrawalDto>> Handle(RequestWithdrawalCommand cmd, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result<WithdrawalDto>.Failure(Error.Validation("Utilisateur non authentifié."));

        var organizerId = Guid.Parse(userId);
        var wallet = await _walletRepository.GetByOrganizerIdAsync(organizerId, cancellationToken);

        if (wallet is null)
            return Result<WithdrawalDto>.Failure(Error.NotFound("Wallet", organizerId));

        var amount = Money.From(cmd.Amount);

        if (amount.Amount > wallet.Balance.Available.Amount)
            return Result<WithdrawalDto>.Failure(Error.Validation("Le montant dépasse le solde disponible."));

        var withdrawal = wallet.RequestWithdrawal(amount, cmd.AccountId);

        await _walletRepository.UpdateAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retrait de {Amount} demandé par l'organisateur {OrganizerId}", cmd.Amount, organizerId);
        return WithdrawalDto.FromDomain(withdrawal);
    }
}
