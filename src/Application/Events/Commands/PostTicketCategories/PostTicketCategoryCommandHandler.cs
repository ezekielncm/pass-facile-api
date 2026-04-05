using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using Domain.Aggregates.Event;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands.PostTicketCategories;

public sealed class PostTicketCategoryCommandHandler
    : IRequestHandler<PostTicketCategoryCommand, Result<CategoryDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PostTicketCategoryCommandHandler> _logger;

    public PostTicketCategoryCommandHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<PostTicketCategoryCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(PostTicketCategoryCommand cmd, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(cmd.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<CategoryDto>.Failure(Error.NotFound("Event", cmd.EventId));

        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        if (@event.OrganizerId != organizerId)
            return Result<CategoryDto>.Failure(Error.Validation("Accès refusé."));

        if (!Enum.TryParse<FeePolicy>(cmd.FeePolicy, true, out var feePolicy))
            return Result<CategoryDto>.Failure(Error.Validation("FeePolicy invalide. Valeurs acceptées : BuyerPays, OrganizerPays."));

        var price = Money.From(cmd.Price);
        var category = TicketCategory.Create(eventId, cmd.Name, price, cmd.Quota, feePolicy, true, cmd.Description);

        @event.AddCategory(category);
        await _eventRepository.UpdateAsync(@event, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Catégorie {CategoryName} ajoutée à l'événement {EventId}", cmd.Name, cmd.EventId);
        return CategoryDto.FromDomain(category);
    }
}
