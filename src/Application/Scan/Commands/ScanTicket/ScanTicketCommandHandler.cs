using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Scan.DTOs;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scan.Commands.ScanTicket;

public sealed class ScanTicketCommandHandler
    : IRequestHandler<ScanTicketCommand, Result<ScanResultDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IScanSessionRepository _scanSessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScanTicketCommandHandler> _logger;

    public ScanTicketCommandHandler(
        ITicketRepository ticketRepository,
        IScanSessionRepository scanSessionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<ScanTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _scanSessionRepository = scanSessionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ScanResultDto>> Handle(ScanTicketCommand cmd, CancellationToken cancellationToken)
    {
        var payload = QRCodePayload.From(cmd.QrPayload);
        var ticketRef = TicketReference.From(payload.TicketRef);
        var ticket = await _ticketRepository.GetByReferenceAsync(ticketRef, cancellationToken);

        if (ticket is null)
        {
            var invalidResult = new ScanResultDto("Invalid", null, Guid.Empty, cmd.ScannedAt);
            return invalidResult;
        }

        if (ticket.Status == TicketStatus.Used)
        {
            var duplicateResult = new ScanResultDto(
                "AlreadyUsed",
                new TicketInfoDto(ticket.Reference.Value, ticket.CategoryId.ToString(), ticket.Status.ToString()),
                Guid.Empty,
                cmd.ScannedAt);
            return duplicateResult;
        }

        if (ticket.Status == TicketStatus.Revoked)
        {
            var invalidResult = new ScanResultDto("Invalid", null, Guid.Empty, cmd.ScannedAt);
            return invalidResult;
        }

        ticket.MarkUsed();
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var validResult = new ScanResultDto(
            "Valid",
            new TicketInfoDto(ticket.Reference.Value, ticket.CategoryId.ToString(), ticket.Status.ToString()),
            Guid.NewGuid(),
            cmd.ScannedAt);

        _logger.LogInformation("Billet {TicketRef} scanné avec succès", ticket.Reference.Value);
        return validResult;
    }
}
