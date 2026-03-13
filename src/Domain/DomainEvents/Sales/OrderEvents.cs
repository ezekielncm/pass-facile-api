using Domain.Common;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.Sales
{
    public sealed record OrderCreated(OrderId OrderId) : Event;

    public sealed record PaymentInitiated(OrderId OrderId, TransactionId TransactionId) : Event;

    public sealed record PaymentConfirmed(OrderId OrderId, TransactionId TransactionId) : Event;

    public sealed record PaymentFailed(OrderId OrderId, TransactionId TransactionId) : Event;

    public sealed record RefundIssued(OrderId OrderId, Money Amount) : Event;

    public sealed record OrderCancelled(OrderId OrderId) : Event;
}

