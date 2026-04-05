using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Orders.DTOs;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandHandler
    : IRequestHandler<InitiatePaymentCommand, Result<PaymentDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InitiatePaymentCommandHandler> _logger;

    public InitiatePaymentCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<InitiatePaymentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(InitiatePaymentCommand cmd, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(cmd.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            return Result<PaymentDto>.Failure(Error.NotFound("Order", cmd.OrderId));

        // Idempotent: if payment already initiated, return existing
        if (order.Payment is not null)
            return PaymentDto.FromDomain(order.Payment);

        if (!Enum.TryParse<PaymentProvider>(cmd.Provider, true, out var provider))
            return Result<PaymentDto>.Failure(Error.Validation("Provider invalide. Valeurs acceptées : OrangeMoney, MoovMoney."));

        var phone = new PhoneNumber(cmd.PaymentPhone);
        var transactionId = TransactionId.From(Guid.NewGuid().ToString("N"));

        order.AddPayment(provider, phone, transactionId, order.Total);

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Paiement initié pour la commande {OrderId} via {Provider}", cmd.OrderId, cmd.Provider);
        return PaymentDto.FromDomain(order.Payment!);
    }
}
