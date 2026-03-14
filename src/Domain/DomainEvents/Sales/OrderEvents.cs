using Domain.Common;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Sales
{
    public sealed record OrderCreated(OrderId OrderId) : DomainEvent;

    public sealed record PaymentInitiated(OrderId OrderId, TransactionId TransactionId) : DomainEvent;

    public sealed record PaymentConfirmed(OrderId OrderId, TransactionId TransactionId) : DomainEvent;

    public sealed record PaymentFailed(OrderId OrderId, TransactionId TransactionId) : DomainEvent;

    public sealed record RefundIssued(OrderId OrderId, Money Amount) : DomainEvent;

    public sealed record OrderCancelled(OrderId OrderId) : DomainEvent;
}

