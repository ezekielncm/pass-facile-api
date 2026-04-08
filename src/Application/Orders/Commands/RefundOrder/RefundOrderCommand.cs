using Application.Common.Models;
using Application.Orders.DTOs;
using MediatR;

namespace Application.Orders.Commands.RefundOrder;

public sealed record RefundOrderCommand(
    Guid OrderId,
    decimal? Amount,
    string Reason) : IRequest<Result<RefundDto>>;
