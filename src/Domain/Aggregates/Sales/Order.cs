using Domain.Common;
using Domain.DomainEvents.Sales;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.Sales
{
    public sealed class Order : AggregateRoot<OrderId>
    {
        private readonly List<OrderItem> _items = [];
        private readonly List<Refund> _refunds = [];

        public Money Total { get; private set; } = Money.From(0);
        public Money ServiceFee { get; private set; } = Money.From(0);
        public Payment? Payment { get; private set; }
        public OrderStatus Status { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();

        // EF
        private Order() { }

        private Order(OrderId id, IEnumerable<OrderItem> items, Money serviceFee)
            : base(id)
        {
            Id = id;
            Status = OrderStatus.Pending;
            _items.AddRange(items);
            ServiceFee = serviceFee;
            Total = CalculateTotal();

            RaiseEvent(new OrderCreated(Id));
        }

        public static Order Create(IEnumerable<OrderItem> items, Money serviceFee)
        {
            var itemList = items.ToList();
            if (!itemList.Any())
            {
                throw new BusinessRuleValidationException("Order.Empty", "Une commande doit contenir au moins un article.");
            }

            return new Order(OrderId.NewId(), itemList, serviceFee);
        }

        public void AddPayment(PaymentMethod method, TransactionId transactionId, Money amount)
        {
            EnsureNotCancelled();

            Payment = Payment.Initiate(Id, method, transactionId, amount);
            RaiseEvent(new PaymentInitiated(Id, transactionId));
        }

        public void MarkPaymentConfirmed()
        {
            EnsureNotCancelled();

            if (Payment is null)
            {
                throw new BusinessRuleValidationException("Order.NoPayment", "Aucun paiement n'est associé à cette commande.");
            }

            Payment.MarkConfirmed();
            RaiseEvent(new PaymentConfirmed(Id, Payment.TransactionId));
        }

        public void MarkPaymentFailed(string reason)
        {
            EnsureNotCancelled();

            if (Payment is null)
            {
                throw new BusinessRuleValidationException("Order.NoPayment", "Aucun paiement n'est associé à cette commande.");
            }

            Payment.MarkFailed(reason);
            RaiseEvent(new PaymentFailed(Id, Payment.TransactionId));
        }

        public void Confirm()
        {
            EnsureNotCancelled();

            if (Payment is null || Payment.Status != PaymentStatus.Confirmed)
            {
                throw new BusinessRuleValidationException("Order.CannotConfirm",
                    "Une commande ne peut être confirmée que si le paiement est confirmé.");
            }

            Status = OrderStatus.Confirmed;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Cancelled)
            {
                return;
            }

            Status = OrderStatus.Cancelled;
            RaiseEvent(new OrderCancelled(Id));
        }

        public Refund IssueRefund(Money amount)
        {
            EnsureNotCancelled();

            if (Payment is null || Payment.Status != PaymentStatus.Confirmed)
            {
                throw new BusinessRuleValidationException("Order.RefundWithoutPayment",
                    "Un remboursement ne peut être émis que si un paiement confirmé existe.");
            }

            var refundedSoFar = _refunds.Aggregate(Money.From(0, amount.Currency),
                (acc, r) => acc.Add(r.Amount));

            if (refundedSoFar.Add(amount) >= Total)
            {
                throw new BusinessRuleValidationException("Order.RefundTooHigh",
                    "Le montant remboursé ne peut excéder le montant original de la commande.");
            }

            var refund = Refund.Create(Id, amount);
            _refunds.Add(refund);

            RaiseEvent(new RefundIssued(Id, amount));
            return refund;
        }

        private void EnsureNotCancelled()
        {
            if (Status == OrderStatus.Cancelled)
            {
                throw new BusinessRuleValidationException("Order.CancelledImmutable",
                    "Une commande annulée ne peut plus être modifiée.");
            }
        }

        private Money CalculateTotal()
        {
            var itemsTotal = _items.Aggregate(Money.From(0, ServiceFee.Currency),
                (acc, item) => acc.Add(item.LineTotal));
            return itemsTotal.Add(ServiceFee);
        }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2
    }

    public sealed class OrderItem : Entity<Guid>
    {
        public OrderId OrderId { get; private set; }
        public string Sku { get; private set; } = null!;
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; } = null!;

        public Money LineTotal => Money.From(UnitPrice.Amount * Quantity, UnitPrice.Currency);

        // EF
        private OrderItem() { }

        private OrderItem(Guid id, OrderId orderId, string sku, int quantity, Money unitPrice)
            : base(id)
        {
            Guard.Against.Null(orderId, nameof(orderId));
            Guard.Against.NullOrEmpty(sku, nameof(sku));

            if (quantity is < 1 or > 10)
            {
                throw new BusinessRuleValidationException("OrderItem.QuantityOutOfRange",
                    "La quantité d'un article doit être comprise entre 1 et 10.");
            }

            OrderId = orderId;
            Sku = sku.Trim();
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public static OrderItem Create(OrderId orderId, string sku, int quantity, Money unitPrice)
        {
            return new OrderItem(Guid.NewGuid(), orderId, sku, quantity, unitPrice);
        }
    }

    public sealed class Payment : Entity<Guid>
    {
        public OrderId OrderId { get; private set; }
        public PaymentMethod Method { get; private set; } = null!;
        public TransactionId TransactionId { get; private set; } = null!;
        public Money Amount { get; private set; } = null!;
        public PaymentStatus Status { get; private set; }
        public string? FailureReason { get; private set; }

        // EF
        private Payment() { }

        private Payment(
            Guid id,
            OrderId orderId,
            PaymentMethod method,
            TransactionId transactionId,
            Money amount)
            : base(id)
        {
            OrderId = orderId;
            Method = method;
            TransactionId = transactionId;
            Amount = amount;
            Status = PaymentStatus.Initiated;
        }

        public static Payment Initiate(
            OrderId orderId,
            PaymentMethod method,
            TransactionId transactionId,
            Money amount)
        {
            return new Payment(Guid.NewGuid(), orderId, method, transactionId, amount);
        }

        public void MarkConfirmed()
        {
            Status = PaymentStatus.Confirmed;
            FailureReason = null;
        }

        public void MarkFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            FailureReason = reason;
        }
    }

    public enum PaymentStatus
    {
        Initiated = 0,
        Confirmed = 1,
        Failed = 2
    }

    public sealed class Refund : Entity<Guid>
    {
        public OrderId OrderId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }

        // EF
        private Refund() { }

        private Refund(Guid id, OrderId orderId, Money amount)
            : base(id)
        {
            OrderId = orderId;
            Amount = amount;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static Refund Create(OrderId orderId, Money amount)
        {
            return new Refund(Guid.NewGuid(), orderId, amount);
        }
    }
}

