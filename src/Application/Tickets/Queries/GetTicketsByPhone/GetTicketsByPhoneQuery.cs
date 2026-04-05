using Application.Common.Models;
using Application.Tickets.DTOs;
using MediatR;

namespace Application.Tickets.Queries.GetTicketsByPhone;

public sealed record GetTicketsByPhoneQuery(string Phone) : IRequest<Result<IReadOnlyCollection<TicketDto>>>;
