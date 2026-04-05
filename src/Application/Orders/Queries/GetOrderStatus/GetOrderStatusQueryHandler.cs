using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Orders.DTOs;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Queries.GetOrderStatus;

public sealed class GetOrderStatusQueryHandler
    : IRequestHandler<GetOrderStatusQuery, Result<OrderStatusDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<GetOrderStatusQueryHandler> _logger;

    public GetOrderStatusQueryHandler(
        IOrderRepository orderRepository,
        ITicketRepository ticketRepository,
        ILogger<GetOrderStatusQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<Result<OrderStatusDto>> Handle(GetOrderStatusQuery query, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(query.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            return Result<OrderStatusDto>.Failure(Error.NotFound("Order", query.OrderId));

        var orderDto = OrderDto.FromDomain(order);
        var paymentStatus = order.Payment?.Status.ToString();

        var tickets = await _ticketRepository.GetByOrderIdAsync(order.Id.Value, cancellationToken);
        var ticketDtos = tickets
            .Select(t => new TicketSummaryDto(t.Id, t.Reference.Value, t.Status.ToString()))
            .ToList()
            .AsReadOnly();

        return new OrderStatusDto(orderDto, paymentStatus, ticketDtos);
    }
}
