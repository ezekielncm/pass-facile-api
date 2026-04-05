using Application.Common.Models;
using MediatR;

namespace Application.Tickets.Queries.GetTicketQr;

public sealed record GetTicketQrQuery(Guid TicketId, string Format = "png") : IRequest<Result<TicketQrDto>>;

public sealed record TicketQrDto(string QrPayload, string Format, string? SignedUrl);
