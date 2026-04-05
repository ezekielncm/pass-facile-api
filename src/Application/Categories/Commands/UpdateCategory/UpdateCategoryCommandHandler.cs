using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand cmd, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.GetAllAsync(cancellationToken);
        var @event = events.FirstOrDefault(e => e.Categories.Any(c => c.Id == cmd.CategoryId));

        if (@event is null)
            return Result<CategoryDto>.Failure(Error.NotFound("Category", cmd.CategoryId));

        var organizerId = _currentUserService.UserId is not null
            ? Guid.Parse(_currentUserService.UserId)
            : Guid.Empty;

        if (@event.OrganizerId != organizerId)
            return Result<CategoryDto>.Failure(Error.Validation("Accès refusé."));

        var category = @event.Categories.First(c => c.Id == cmd.CategoryId);

        if (cmd.Price.HasValue && category.SoldCount > 0)
            return Result<CategoryDto>.Failure(Error.Validation("Le prix ne peut pas être modifié si des billets ont été vendus."));

        if (cmd.Quota.HasValue && cmd.Quota.Value < category.SoldCount)
            return Result<CategoryDto>.Failure(Error.Validation("Le quota ne peut pas être inférieur au nombre de billets vendus."));

        // TODO: Apply property updates once domain mutation methods are added.
        // TicketCategory uses private setters; a domain Update method should be
        // introduced to mutate Name, Price, Quota, and IsActive.
        if (cmd.IsActive.HasValue)
        {
            if (cmd.IsActive.Value)
                category.Activate();
            else
                category.Deactivate();
        }

        await _eventRepository.UpdateAsync(@event, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Catégorie {CategoryId} mise à jour", cmd.CategoryId);
        return CategoryDto.FromDomain(category);
    }
}
