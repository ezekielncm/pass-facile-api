using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Scan.DTOs;
using Domain.Enums;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scan.Queries.GetOfflineBundle;

public sealed class GetOfflineBundleQueryHandler
    : IRequestHandler<GetOfflineBundleQuery, Result<OfflineBundleDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<GetOfflineBundleQueryHandler> _logger;

    public GetOfflineBundleQueryHandler(
        IEventRepository eventRepository,
        ITicketRepository ticketRepository,
        ILogger<GetOfflineBundleQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<Result<OfflineBundleDto>> Handle(GetOfflineBundleQuery query, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(query.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<OfflineBundleDto>.Failure(Error.NotFound("Event", query.EventId));

        if (@event.StartDate - DateTimeOffset.UtcNow > TimeSpan.FromHours(24))
            return Result<OfflineBundleDto>.Failure(Error.Validation("Le bundle hors ligne n'est disponible que 24h avant l'événement."));

        var tickets = await _ticketRepository.GetByEventIdAsync(query.EventId, cancellationToken);
        var categories = @event.Categories.ToDictionary(c => c.Id, c => c.Name);

        var offlineTickets = tickets
            .Where(t => t.Status == TicketStatus.Issued && t.QRCode is not null)
            .Select(t => new OfflineTicketDto(
                t.QRCode!.Payload.Encode(),
                t.Reference.Value,
                categories.TryGetValue(t.CategoryId, out var catName) ? catName : "Inconnu"))
            .ToList()
            .AsReadOnly();

        var signature = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{query.EventId}:{DateTimeOffset.UtcNow:o}"));

        return new OfflineBundleDto(offlineTickets, DateTimeOffset.UtcNow, signature);
    }
}
