using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Tickets.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Tickets.Queries.GetTicketsByPhone;

public sealed class GetTicketsByPhoneQueryHandler
    : IRequestHandler<GetTicketsByPhoneQuery, Result<IReadOnlyCollection<TicketDto>>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<GetTicketsByPhoneQueryHandler> _logger;

    public GetTicketsByPhoneQueryHandler(
        ITicketRepository ticketRepository,
        IEventRepository eventRepository,
        ILogger<GetTicketsByPhoneQueryHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyCollection<TicketDto>>> Handle(GetTicketsByPhoneQuery query, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.GetAllAsync(cancellationToken);
        var futureEventIds = events
            .Where(e => e.EndDate > DateTimeOffset.UtcNow)
            .Select(e => e.Id.Value)
            .ToHashSet();

        var allTickets = new List<Domain.Aggregates.Ticketing.Ticket>();
        foreach (var eventId in futureEventIds)
        {
            var tickets = await _ticketRepository.GetByEventIdAsync(eventId, cancellationToken);
            allTickets.AddRange(tickets.Where(t => t.BuyerPhone.Value == query.Phone));
        }

        var ticketDtos = allTickets
            .Select(TicketDto.FromDomain)
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("{Count} billets trouvés pour {Phone}", ticketDtos.Count, query.Phone);
        return Result<IReadOnlyCollection<TicketDto>>.Success(ticketDtos);
    }
}
