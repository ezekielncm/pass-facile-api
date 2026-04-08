using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Tickets.Queries.GetTicketQr;

public sealed class GetTicketQrQueryHandler
    : IRequestHandler<GetTicketQrQuery, Result<TicketQrDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<GetTicketQrQueryHandler> _logger;

    public GetTicketQrQueryHandler(
        ITicketRepository ticketRepository,
        ILogger<GetTicketQrQueryHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<Result<TicketQrDto>> Handle(GetTicketQrQuery query, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(query.TicketId, cancellationToken);

        if (ticket is null)
            return Result<TicketQrDto>.Failure(Error.NotFound("Ticket", query.TicketId));

        if (ticket.QRCode is null)
            return Result<TicketQrDto>.Failure(Error.Validation("Ce billet n'a pas de QR code généré."));

        var payload = ticket.QRCode.Payload.Encode();

        return new TicketQrDto(payload, query.Format, null);
    }
}
