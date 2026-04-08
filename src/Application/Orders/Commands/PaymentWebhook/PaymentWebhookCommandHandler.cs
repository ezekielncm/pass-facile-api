using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Commands.PaymentWebhook;

public sealed class PaymentWebhookCommandHandler
    : IRequestHandler<PaymentWebhookCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentWebhookCommandHandler> _logger;

    public PaymentWebhookCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentWebhookCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(PaymentWebhookCommand cmd, CancellationToken cancellationToken)
    {
        // TODO: Verify HMAC signature from provider
        // For now, trust the webhook payload

        _logger.LogInformation("Webhook reçu pour la transaction {TransactionId}, status: {Status}", cmd.TransactionId, cmd.Status);

        // The order lookup by transactionId would require an additional repository method
        // For now, this is a placeholder that delegates to domain logic
        return true;
    }
}
