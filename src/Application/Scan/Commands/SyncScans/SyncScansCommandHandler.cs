using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scan.Commands.SyncScans;

public sealed class SyncScansCommandHandler
    : IRequestHandler<SyncScansCommand, Result<SyncResultDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncScansCommandHandler> _logger;

    public SyncScansCommandHandler(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        ILogger<SyncScansCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SyncResultDto>> Handle(SyncScansCommand cmd, CancellationToken cancellationToken)
    {
        var synced = 0;
        var conflicts = new List<SyncConflictDto>();

        var orderedScans = cmd.Scans.OrderBy(s => s.ScannedAt).ToList();

        foreach (var scan in orderedScans)
        {
            var payload = QRCodePayload.From(scan.QrPayload);
            var ticketRef = TicketReference.From(payload.TicketRef);
            var ticket = await _ticketRepository.GetByReferenceAsync(ticketRef, cancellationToken);

            if (ticket is null)
            {
                conflicts.Add(new SyncConflictDto(scan.QrPayload, "Billet introuvable."));
                continue;
            }

            if (ticket.Status == TicketStatus.Used)
            {
                conflicts.Add(new SyncConflictDto(scan.QrPayload, "Billet déjà scanné par un autre agent."));
                continue;
            }

            ticket.MarkUsed();
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            synced++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{Synced} scans synchronisés, {Conflicts} conflits", synced, conflicts.Count);
        return new SyncResultDto(synced, conflicts.AsReadOnly());
    }
}
