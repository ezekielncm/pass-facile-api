using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.Aggregates.Event;
using Domain.Enums;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands.CreatePromoCode;

public sealed class CreatePromoCodeCommandHandler
    : IRequestHandler<CreatePromoCodeCommand, Result<PromoCodeDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePromoCodeCommandHandler> _logger;

    public CreatePromoCodeCommandHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CreatePromoCodeCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PromoCodeDto>> Handle(CreatePromoCodeCommand cmd, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(cmd.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<PromoCodeDto>.Failure(Error.NotFound("Event", cmd.EventId));

        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        if (@event.OrganizerId != organizerId)
            return Result<PromoCodeDto>.Failure(Error.Validation("Accès refusé."));

        if (!Enum.TryParse<DiscountType>(cmd.DiscountType, true, out var discountType))
            return Result<PromoCodeDto>.Failure(Error.Validation("DiscountType invalide. Valeurs acceptées : Percent, Fixed."));

        if (discountType == DiscountType.Percent && cmd.Value > 100)
            return Result<PromoCodeDto>.Failure(Error.Validation("La valeur d'une réduction en pourcentage ne peut pas dépasser 100."));

        if (cmd.ExpiresAt > @event.EndDate)
            return Result<PromoCodeDto>.Failure(Error.Validation("La date d'expiration du code promo ne peut pas dépasser la date de l'événement."));

        if (@event.PromoCodes.Any(p => p.Code.Equals(cmd.Code, StringComparison.OrdinalIgnoreCase)))
            return Result<PromoCodeDto>.Failure(Error.Conflict("Un code promo avec ce code existe déjà pour cet événement."));

        var promoCode = PromoCode.Create(eventId, cmd.Code, discountType, cmd.Value, cmd.MaxUses, cmd.ExpiresAt);
        @event.AddPromoCode(promoCode);
        await _eventRepository.UpdateAsync(@event, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Code promo {Code} créé pour l'événement {EventId}", cmd.Code, cmd.EventId);
        return PromoCodeDto.FromDomain(promoCode);
    }
}
