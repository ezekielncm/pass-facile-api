using Application.Common.Models;
using Application.Orders.DTOs;
using MediatR;

namespace Application.Orders.Queries.GetOrderStatus;

public sealed record GetOrderStatusQuery(Guid OrderId) : IRequest<Result<OrderStatusDto>>;

public sealed record OrderStatusDto(
    OrderDto Order,
    string? PaymentStatus,
    IReadOnlyCollection<TicketSummaryDto> Tickets);

public sealed record TicketSummaryDto(
    Guid Id,
    string Reference,
    string Status);
