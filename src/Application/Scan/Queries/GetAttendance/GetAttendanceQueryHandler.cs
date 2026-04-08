using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Scan.DTOs;
using Domain.Enums;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scan.Queries.GetAttendance;

public sealed class GetAttendanceQueryHandler
    : IRequestHandler<GetAttendanceQuery, Result<AttendanceDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IScanSessionRepository _scanSessionRepository;
    private readonly ILogger<GetAttendanceQueryHandler> _logger;

    public GetAttendanceQueryHandler(
        IEventRepository eventRepository,
        ITicketRepository ticketRepository,
        IScanSessionRepository scanSessionRepository,
        ILogger<GetAttendanceQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _ticketRepository = ticketRepository;
        _scanSessionRepository = scanSessionRepository;
        _logger = logger;
    }

    public async Task<Result<AttendanceDto>> Handle(GetAttendanceQuery query, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(query.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<AttendanceDto>.Failure(Error.NotFound("Event", query.EventId));

        var tickets = await _ticketRepository.GetByEventIdAsync(query.EventId, cancellationToken);
        var usedTickets = tickets.Where(t => t.Status == TicketStatus.Used).ToList();

        var byCategory = @event.Categories.Select(c =>
        {
            var scanned = usedTickets.Count(t => t.CategoryId == c.Id);
            return new CategoryAttendanceDto(c.Name, scanned, c.Quota);
        }).ToList().AsReadOnly();

        var sessions = await _scanSessionRepository.GetByEventIdAsync(query.EventId, cancellationToken);
        var recentEntries = sessions
            .SelectMany(s => s.Events)
            .Where(e => e.Result.Status == ScanStatus.Valid)
            .OrderByDescending(e => e.ScannedAt)
            .Take(20)
            .Select(e => e.ScannedAt)
            .ToList()
            .AsReadOnly();

        return new AttendanceDto(usedTickets.Count, byCategory, recentEntries);
    }
}
