using Domain.Common;
using Domain.DomainEvents.Sales;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.Sales
{
    public sealed class Order : AggregateRoot<OrderId>
    {
        private readonly List<OrderItem> _items = [];
        private readonly List<Refund> _refunds = [];

        public PhoneNumber BuyerPhone { get; private set; } = null!;
        public Guid CategoryId { get; private set; }
        public Guid EventId { get; private set; }
        public Guid? PromoCodeId { get; private set; }
        public int Quantity { get; private set; }
        public Money Subtotal { get; private set; } = Money.From(0);
        public Money Fees { get; private set; } = Money.From(0);
        public Money Total { get; private set; } = Money.From(0);
        public OrderStatus Status { get; private set; }
        public DateTimeOffset ReservedUntil { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public Payment? Payment { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();

        // EF
        private Order() { }

        private Order(
            OrderId id,
            PhoneNumber buyerPhone,
            Guid categoryId,
            Guid eventId,
            Guid? promoCodeId,
            int quantity,
            IEnumerable<OrderItem> items,
            Money fees,
            DateTimeOffset reservedUntil)
            : base(id)
        {
            Id = id;
            BuyerPhone = buyerPhone;
            CategoryId = categoryId;
            EventId = eventId;
            PromoCodeId = promoCodeId;
            Quantity = quantity;
            Status = OrderStatus.Pending;
            Fees = fees;
            ReservedUntil = reservedUntil;
            CreatedAt = DateTimeOffset.UtcNow;
            _items.AddRange(items);
            Subtotal = CalculateSubtotal();
            Total = Subtotal.Add(Fees);

            RaiseEvent(new OrderCreated(Id));
        }

        public static Order Create(
            PhoneNumber buyerPhone,
            Guid categoryId,
            Guid eventId,
            int quantity,
            IEnumerable<OrderItem> items,
            Money fees,
            DateTimeOffset reservedUntil,
            Guid? promoCodeId = null)
        {
            Guard.Against.Null(buyerPhone, nameof(buyerPhone));
            var itemList = items.ToList();
            if (!itemList.Any())
            {
                throw new BusinessRuleValidationException("Order.Empty", "Une commande doit contenir au moins un article.");
            }

            return new Order(OrderId.NewId(), buyerPhone, categoryId, eventId, promoCodeId, quantity, itemList, fees, reservedUntil);
        }

        public void AddPayment(PaymentProvider provider, PhoneNumber phoneNumber, TransactionId transactionId, Money amount)
        {
            EnsureNotCancelled();

            Payment = Payment.Initiate(Id, provider, phoneNumber, transactionId, amount);
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

        public Refund IssueRefund(Guid paymentId, Money amount, string reason)
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

            var refund = Refund.Create(Id, paymentId, amount, reason);
            _refunds.Add(refund);

            RaiseEvent(new RefundIssued(Id, amount));
            return refund;
        }

        public void ApplyPromo(Guid promoCodeId)
        {
            PromoCodeId = promoCodeId;
        }

        public Money ComputeFees(FeePolicy policy)
        {
            return Fees;
        }

        private void EnsureNotCancelled()
        {
            if (Status == OrderStatus.Cancelled)
            {
                throw new BusinessRuleValidationException("Order.CancelledImmutable",
                    "Une commande annulée ne peut plus être modifiée.");
            }
        }

        private Money CalculateSubtotal()
        {
            return _items.Aggregate(Money.From(0, Fees.Currency),
                (acc, item) => acc.Add(item.LineTotal));
        }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Reserved = 1,
        Confirmed = 2,
        Cancelled = 3,
        Refunded = 4
    }

    public sealed class OrderItem : Entity<Guid>
    {
        public OrderId OrderId { get; private set; }
        public TicketReference TicketRef { get; private set; } = null!;
        public Money UnitPrice { get; private set; } = null!;
        public int Quantity { get; private set; }

        public Money LineTotal => Money.From(UnitPrice.Amount * Quantity, UnitPrice.Currency);

        // EF
        private OrderItem() { }

        private OrderItem(Guid id, OrderId orderId, TicketReference ticketRef, int quantity, Money unitPrice)
            : base(id)
        {
            Guard.Against.Null(orderId, nameof(orderId));
            Guard.Against.Null(ticketRef, nameof(ticketRef));

            if (quantity is < 1 or > 10)
            {
                throw new BusinessRuleValidationException("OrderItem.QuantityOutOfRange",
                    "La quantité d'un article doit être comprise entre 1 et 10.");
            }

            OrderId = orderId;
            TicketRef = ticketRef;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public static OrderItem Create(OrderId orderId, TicketReference ticketRef, int quantity, Money unitPrice)
        {
            return new OrderItem(Guid.NewGuid(), orderId, ticketRef, quantity, unitPrice);
        }
    }

    public sealed class Payment : Entity<Guid>
    {
        public OrderId OrderId { get; private set; }
        public PaymentProvider Provider { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public Money Amount { get; private set; } = null!;
        public TransactionId TransactionId { get; private set; } = null!;
        public PaymentStatus Status { get; private set; }
        public DateTimeOffset InitiatedAt { get; private set; }
        public DateTimeOffset? ConfirmedAt { get; private set; }
        public string? FailureReason { get; private set; }

        // EF
        private Payment() { }

        private Payment(
            Guid id,
            OrderId orderId,
            PaymentProvider provider,
            PhoneNumber phoneNumber,
            TransactionId transactionId,
            Money amount)
            : base(id)
        {
            OrderId = orderId;
            Provider = provider;
            PhoneNumber = phoneNumber;
            TransactionId = transactionId;
            Amount = amount;
            Status = PaymentStatus.Initiated;
            InitiatedAt = DateTimeOffset.UtcNow;
        }

        public static Payment Initiate(
            OrderId orderId,
            PaymentProvider provider,
            PhoneNumber phoneNumber,
            TransactionId transactionId,
            Money amount)
        {
            return new Payment(Guid.NewGuid(), orderId, provider, phoneNumber, transactionId, amount);
        }

        public void MarkConfirmed()
        {
            Status = PaymentStatus.Confirmed;
            ConfirmedAt = DateTimeOffset.UtcNow;
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
        Failed = 2,
        Expired = 3
    }

    public sealed class Refund : Entity<Guid>
    {
        public OrderId OrderId { get; private set; }
        public Guid PaymentId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public string Reason { get; private set; } = null!;
        public RefundStatus Status { get; private set; }
        public DateTimeOffset? ProcessedAt { get; private set; }

        // EF
        private Refund() { }

        private Refund(Guid id, OrderId orderId, Guid paymentId, Money amount, string reason)
            : base(id)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
            Reason = reason;
            Status = Enums.RefundStatus.Requested;
        }

        public static Refund Create(OrderId orderId, Guid paymentId, Money amount, string reason)
        {
            return new Refund(Guid.NewGuid(), orderId, paymentId, amount, reason);
        }

        public void Process()
        {
            Status = Enums.RefundStatus.Processed;
            ProcessedAt = DateTimeOffset.UtcNow;
        }
    }
}

