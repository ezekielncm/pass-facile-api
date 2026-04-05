using Application.Common.Models;
using Application.Orders.DTOs;
using MediatR;

namespace Application.Orders.Commands.InitiatePayment;

public sealed record InitiatePaymentCommand(
    Guid OrderId,
    string PaymentPhone,
    string Provider) : IRequest<Result<PaymentDto>>;
