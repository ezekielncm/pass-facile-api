using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Orders.DTOs;
using Domain.Aggregates.Sales;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand cmd, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.GetAllAsync(cancellationToken);
        var @event = events.FirstOrDefault(e => e.Categories.Any(c => c.Id == cmd.CategoryId));

        if (@event is null)
            return Result<OrderDto>.Failure(Error.NotFound("Category", cmd.CategoryId));

        var category = @event.Categories.First(c => c.Id == cmd.CategoryId);

        if (!category.IsActive)
            return Result<OrderDto>.Failure(Error.Validation("Cette catégorie n'est plus active."));

        if (category.SoldCount + cmd.Quantity > category.Quota)
            return Result<OrderDto>.Failure(Error.Validation("Stock insuffisant pour cette catégorie."));

        var buyerPhone = new PhoneNumber(cmd.BuyerPhone);
        var unitPrice = category.Price;
        var orderId = OrderId.NewId();
        var ticketRef = TicketReference.From($"TKT-{Guid.NewGuid():N}"[..16].ToUpperInvariant());
        var item = OrderItem.Create(orderId, ticketRef, cmd.Quantity, unitPrice);
        var fees = Money.From(0);
        var reservedUntil = DateTimeOffset.UtcNow.AddMinutes(10);

        var order = Order.Create(
            buyerPhone, cmd.CategoryId, @event.Id.Value, cmd.Quantity,
            new[] { item }, fees, reservedUntil);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commande {OrderId} créée pour {Quantity} billets", order.Id.Value, cmd.Quantity);
        return OrderDto.FromDomain(order);
    }
}
