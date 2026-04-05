using Application.Common.Models;
using Application.Orders.DTOs;
using MediatR;

namespace Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CategoryId,
    int Quantity,
    string BuyerPhone,
    string? PromoCode) : IRequest<Result<OrderDto>>;
