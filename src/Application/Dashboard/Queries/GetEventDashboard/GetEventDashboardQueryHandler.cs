using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Dashboard.DTOs;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Dashboard.Queries.GetEventDashboard;

public sealed class GetEventDashboardQueryHandler
    : IRequestHandler<GetEventDashboardQuery, Result<DashboardDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IOrganizerWalletRepository _walletRepository;
    private readonly ILogger<GetEventDashboardQueryHandler> _logger;

    public GetEventDashboardQueryHandler(
        IEventRepository eventRepository,
        ITicketRepository ticketRepository,
        IOrganizerWalletRepository walletRepository,
        ILogger<GetEventDashboardQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _ticketRepository = ticketRepository;
        _walletRepository = walletRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardDto>> Handle(GetEventDashboardQuery query, CancellationToken cancellationToken)
    {
        var eventId = Domain.ValueObjects.Identities.EventId.From(query.EventId);
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
            return Result<DashboardDto>.Failure(Error.NotFound("Event", query.EventId));

        var tickets = await _ticketRepository.GetByEventIdAsync(query.EventId, cancellationToken);
        var categories = @event.Categories.ToDictionary(c => c.Id, c => c);

        var soldTickets = tickets.Count;

        // Sales by category
        var byCategory = @event.Categories.Select(c => new SalesByCategoryDto(
            c.Name,
            c.SoldCount,
            c.Quota,
            c.Price.Amount * c.SoldCount
        )).ToList().AsReadOnly();

        var grossRevenue = byCategory.Sum(c => c.Revenue);

        // Get wallet for net revenue
        var wallet = await _walletRepository.GetByOrganizerIdAsync(@event.OrganizerId, cancellationToken);
        var fees = wallet?.Fees
            .Where(f => f.EventId == query.EventId)
            .ToList() ?? [];

        var platformFees = fees.Sum(f => f.PlatformFee.Amount);
        var netRevenue = grossRevenue - platformFees;

        // Sales by day from ticket issuance
        var salesByDay = tickets
            .GroupBy(t => t.IssuedAt.Date)
            .Select(g => new SalesByDayDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Count(),
                g.Sum(t => categories.TryGetValue(t.CategoryId, out var cat) ? cat.Price.Amount : 0)))
            .OrderByDescending(s => s.Date)
            .ToList()
            .AsReadOnly();

        return new DashboardDto(
            soldTickets, grossRevenue, netRevenue, "XOF", salesByDay, byCategory);
    }
}
