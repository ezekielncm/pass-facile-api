using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Dashboard.DTOs;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Dashboard.Queries.ExportParticipants;

public sealed class ExportParticipantsQueryHandler
    : IRequestHandler<ExportParticipantsQuery, Result<ParticipantExportDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<ExportParticipantsQueryHandler> _logger;

    public ExportParticipantsQueryHandler(
        IEventRepository eventRepository,
        ITicketRepository ticketRepository,
        ILogger<ExportParticipantsQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<Result<ParticipantExportDto>> Handle(ExportParticipantsQuery query, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(query.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<ParticipantExportDto>.Failure(Error.NotFound("Event", query.EventId));

        var tickets = await _ticketRepository.GetByEventIdAsync(query.EventId, cancellationToken);
        var categories = @event.Categories.ToDictionary(c => c.Id, c => c.Name);

        if (query.CategoryId.HasValue)
            tickets = tickets.Where(t => t.CategoryId == query.CategoryId.Value).ToList();

        if (query.Format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Téléphone,Catégorie,Statut,Référence");

            foreach (var ticket in tickets)
            {
                var categoryName = categories.TryGetValue(ticket.CategoryId, out var name) ? name : "Inconnu";
                sb.AppendLine($"{ticket.BuyerPhone.Value},{categoryName},{ticket.Status},{ticket.Reference.Value}");
            }

            var content = Encoding.UTF8.GetBytes(sb.ToString());
            return new ParticipantExportDto(content, "text/csv", $"participants-{query.EventId}.csv");
        }

        // PDF format placeholder
        return Result<ParticipantExportDto>.Failure(Error.Validation("Le format PDF n'est pas encore supporté."));
    }
}
