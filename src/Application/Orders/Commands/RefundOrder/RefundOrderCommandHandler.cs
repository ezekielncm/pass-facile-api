using Application.Common.Interfaces.Persistence;
using Application.Common.Models;
using Application.Orders.DTOs;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Commands.RefundOrder;

public sealed class RefundOrderCommandHandler
    : IRequestHandler<RefundOrderCommand, Result<RefundDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefundOrderCommandHandler> _logger;

    public RefundOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<RefundOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<RefundDto>> Handle(RefundOrderCommand cmd, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(cmd.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            return Result<RefundDto>.Failure(Error.NotFound("Order", cmd.OrderId));

        if (order.Payment is null)
            return Result<RefundDto>.Failure(Error.Validation("Aucun paiement trouvé pour cette commande."));

        var refundAmount = cmd.Amount.HasValue
            ? Money.From(cmd.Amount.Value)
            : order.Total;

        if (refundAmount.Amount > order.Total.Amount)
            return Result<RefundDto>.Failure(Error.Validation("Le montant du remboursement ne peut pas dépasser le montant payé."));

        var refund = order.IssueRefund(order.Payment.Id, refundAmount, cmd.Reason);

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Remboursement de {Amount} émis pour la commande {OrderId}", refundAmount.Amount, cmd.OrderId);
        return RefundDto.FromDomain(refund);
    }
}
