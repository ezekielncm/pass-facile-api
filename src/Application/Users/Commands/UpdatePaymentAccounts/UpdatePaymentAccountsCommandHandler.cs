using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Users.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Users.Commands.UpdatePaymentAccounts;

public sealed class UpdatePaymentAccountsCommandHandler
    : IRequestHandler<UpdatePaymentAccountsCommand, Result<PaymentAccountsDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePaymentAccountsCommandHandler> _logger;

    public UpdatePaymentAccountsCommandHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePaymentAccountsCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PaymentAccountsDto>> Handle(UpdatePaymentAccountsCommand cmd, CancellationToken cancellationToken)
    {
        if (cmd.OrangeMoneyNumber is null && cmd.MoovMoneyNumber is null)
            return Result<PaymentAccountsDto>.Failure(Error.Validation("Au moins un compte Mobile Money est requis."));

        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result<PaymentAccountsDto>.Failure(Error.Validation("Utilisateur non authentifié."));

        var id = Domain.ValueObjects.Identities.UserId.FromGuid(Guid.Parse(userId));
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result<PaymentAccountsDto>.Failure(Error.NotFound("User", userId));

        // Delegate to domain - payment account update logic
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment accounts updated for user {UserId}", userId);
        return new PaymentAccountsDto(cmd.OrangeMoneyNumber, cmd.MoovMoneyNumber);
    }
}
