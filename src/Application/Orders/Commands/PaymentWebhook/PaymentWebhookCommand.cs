using Application.Common.Models;
using MediatR;

namespace Application.Orders.Commands.PaymentWebhook;

public sealed record PaymentWebhookCommand(
    string TransactionId,
    string Status,
    string? FailureReason,
    string Signature) : IRequest<Result<bool>>;
