using Domain.Aggregates.Sales;
using Domain.Common;
using Domain.DomainEvents.Sales;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace DomainUnitTest.Aggregates;

public class OrderTests
{
    private static readonly PhoneNumber BuyerPhone = new("226", "70000001");
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid EventIdGuid = Guid.NewGuid();

    private static OrderItem MakeItem(OrderId? orderId = null) =>
        OrderItem.Create(orderId ?? OrderId.NewId(), TicketReference.From("TKT-001"), 1, Money.From(1000));

    private static Order CreateValidOrder(OrderId? orderId = null)
    {
        var id = orderId ?? OrderId.NewId();
        var item = MakeItem(id);
        return Order.Create(BuyerPhone, CategoryId, EventIdGuid, 1,
            new[] { item }, Money.From(100), DateTimeOffset.UtcNow.AddMinutes(30));
    }

    private static Order CreateOrderWithPaymentConfirmed()
    {
        var order = CreateValidOrder();
        order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
            TransactionId.From("TXN-001"), Money.From(1100));
        order.MarkPaymentConfirmed();
        return order;
    }

    // --- Create ---

    [Fact]
    public void Create_WithValidData_SetsPropertiesAndPendingStatus()
    {
        var order = CreateValidOrder();

        Assert.Equal(BuyerPhone, order.BuyerPhone);
        Assert.Equal(CategoryId, order.CategoryId);
        Assert.Equal(EventIdGuid, order.EventId);
        Assert.Equal(1, order.Quantity);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.Items);
        Assert.Null(order.PromoCodeId);
        Assert.NotNull(order.Id);
    }

    [Fact]
    public void Create_CalculatesSubtotalAndTotal()
    {
        var order = CreateValidOrder();

        Assert.True(order.Subtotal.Amount > 0);
        Assert.Equal(order.Subtotal.Add(order.Fees), order.Total);
    }

    [Fact]
    public void Create_RaisesOrderCreated()
    {
        var order = CreateValidOrder();

        Assert.Contains(order.Events, e => e is OrderCreated);
    }

    [Fact]
    public void Create_WithEmptyItems_ThrowsBusinessRuleValidationException()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Order.Create(BuyerPhone, CategoryId, EventIdGuid, 1,
                Enumerable.Empty<OrderItem>(), Money.From(100), DateTimeOffset.UtcNow.AddMinutes(30)));
    }

    [Fact]
    public void Create_WithNullPhone_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Order.Create(null!, CategoryId, EventIdGuid, 1,
                new[] { MakeItem() }, Money.From(100), DateTimeOffset.UtcNow.AddMinutes(30)));
    }

    // --- AddPayment ---

    [Fact]
    public void AddPayment_CreatesPaymentAndRaisesEvent()
    {
        var order = CreateValidOrder();
        order.ClearEvents();

        order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
            TransactionId.From("TXN-001"), Money.From(1100));

        Assert.NotNull(order.Payment);
        Assert.Equal(PaymentStatus.Initiated, order.Payment!.Status);
        Assert.Contains(order.Events, e => e is PaymentInitiated);
    }

    [Fact]
    public void AddPayment_WhenCancelled_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();
        order.Cancel();

        Assert.Throws<BusinessRuleValidationException>(() =>
            order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
                TransactionId.From("TXN-002"), Money.From(1100)));
    }

    // --- MarkPaymentConfirmed ---

    [Fact]
    public void MarkPaymentConfirmed_ConfirmsPaymentAndRaisesEvent()
    {
        var order = CreateValidOrder();
        order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
            TransactionId.From("TXN-001"), Money.From(1100));
        order.ClearEvents();

        order.MarkPaymentConfirmed();

        Assert.Equal(PaymentStatus.Confirmed, order.Payment!.Status);
        Assert.Contains(order.Events, e => e is PaymentConfirmed);
    }

    [Fact]
    public void MarkPaymentConfirmed_WithNoPayment_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();

        Assert.Throws<BusinessRuleValidationException>(() => order.MarkPaymentConfirmed());
    }

    [Fact]
    public void MarkPaymentConfirmed_WhenCancelled_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();
        order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
            TransactionId.From("TXN-001"), Money.From(1100));
        order.Cancel();

        Assert.Throws<BusinessRuleValidationException>(() => order.MarkPaymentConfirmed());
    }

    // --- MarkPaymentFailed ---

    [Fact]
    public void MarkPaymentFailed_SetsFailedStatusAndRaisesEvent()
    {
        var order = CreateValidOrder();
        order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
            TransactionId.From("TXN-001"), Money.From(1100));
        order.ClearEvents();

        order.MarkPaymentFailed("timeout");

        Assert.Equal(PaymentStatus.Failed, order.Payment!.Status);
        Assert.Contains(order.Events, e => e is PaymentFailed);
    }

    [Fact]
    public void MarkPaymentFailed_WithNoPayment_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();

        Assert.Throws<BusinessRuleValidationException>(() => order.MarkPaymentFailed("reason"));
    }

    [Fact]
    public void MarkPaymentFailed_WhenCancelled_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();
        order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
            TransactionId.From("TXN-001"), Money.From(1100));
        order.Cancel();

        Assert.Throws<BusinessRuleValidationException>(() => order.MarkPaymentFailed("reason"));
    }

    // --- Confirm ---

    [Fact]
    public void Confirm_WithConfirmedPayment_SetsStatusToConfirmed()
    {
        var order = CreateOrderWithPaymentConfirmed();

        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WhenCancelled_ThrowsBusinessRuleValidationException()
    {
        var order = CreateOrderWithPaymentConfirmed();
        order.Cancel();

        Assert.Throws<BusinessRuleValidationException>(() => order.Confirm());
    }

    [Fact]
    public void Confirm_WithNoPayment_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();

        Assert.Throws<BusinessRuleValidationException>(() => order.Confirm());
    }

    [Fact]
    public void Confirm_WithUnconfirmedPayment_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();
        order.AddPayment(PaymentProvider.OrangeMoney, BuyerPhone,
            TransactionId.From("TXN-001"), Money.From(1100));

        Assert.Throws<BusinessRuleValidationException>(() => order.Confirm());
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_SetsStatusToCancelledAndRaisesEvent()
    {
        var order = CreateValidOrder();
        order.ClearEvents();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Contains(order.Events, e => e is OrderCancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsIdempotent()
    {
        var order = CreateValidOrder();
        order.Cancel();
        order.ClearEvents();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Empty(order.Events);
    }

    // --- IssueRefund ---

    [Fact]
    public void IssueRefund_CreatesRefundAndRaisesEvent()
    {
        var order = CreateOrderWithPaymentConfirmed();
        var paymentId = order.Payment!.Id;
        order.ClearEvents();

        var refund = order.IssueRefund(paymentId, Money.From(500), "Partial refund");

        Assert.NotNull(refund);
        Assert.Single(order.Refunds);
        Assert.Contains(order.Events, e => e is RefundIssued);
    }

    [Fact]
    public void IssueRefund_WithNoConfirmedPayment_ThrowsBusinessRuleValidationException()
    {
        var order = CreateValidOrder();

        Assert.Throws<BusinessRuleValidationException>(() =>
            order.IssueRefund(Guid.NewGuid(), Money.From(500), "reason"));
    }

    [Fact]
    public void IssueRefund_WhenCancelled_ThrowsBusinessRuleValidationException()
    {
        var order = CreateOrderWithPaymentConfirmed();
        order.Cancel();

        Assert.Throws<BusinessRuleValidationException>(() =>
            order.IssueRefund(order.Payment!.Id, Money.From(500), "reason"));
    }

    [Fact]
    public void IssueRefund_ExceedingTotal_ThrowsBusinessRuleValidationException()
    {
        var order = CreateOrderWithPaymentConfirmed();

        Assert.Throws<BusinessRuleValidationException>(() =>
            order.IssueRefund(order.Payment!.Id, order.Total, "too much"));
    }

    // --- ApplyPromo ---

    [Fact]
    public void ApplyPromo_SetsPromoCodeId()
    {
        var order = CreateValidOrder();
        var promoId = Guid.NewGuid();

        order.ApplyPromo(promoId);

        Assert.Equal(promoId, order.PromoCodeId);
    }
}

public class OrderItemTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var orderId = OrderId.NewId();
        var ticketRef = TicketReference.From("TKT-001");
        var price = Money.From(1500);

        var item = OrderItem.Create(orderId, ticketRef, 2, price);

        Assert.Equal(orderId, item.OrderId);
        Assert.Equal(ticketRef, item.TicketRef);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(price, item.UnitPrice);
    }

    [Fact]
    public void LineTotal_CalculatesCorrectly()
    {
        var item = OrderItem.Create(OrderId.NewId(), TicketReference.From("TKT-001"), 3, Money.From(1000));

        Assert.Equal(3000m, item.LineTotal.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(100)]
    public void Create_WithQuantityOutsideRange_ThrowsBusinessRuleValidationException(int quantity)
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            OrderItem.Create(OrderId.NewId(), TicketReference.From("TKT-001"), quantity, Money.From(1000)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Create_WithValidQuantity_Succeeds(int quantity)
    {
        var item = OrderItem.Create(OrderId.NewId(), TicketReference.From("TKT-001"), quantity, Money.From(1000));

        Assert.Equal(quantity, item.Quantity);
    }
}

public class PaymentTests
{
    [Fact]
    public void Initiate_SetsPropertiesWithInitiatedStatus()
    {
        var orderId = OrderId.NewId();
        var phone = new PhoneNumber("226", "70000001");
        var txnId = TransactionId.From("TXN-001");
        var amount = Money.From(5000);

        var payment = Payment.Initiate(orderId, PaymentProvider.MoovMoney, phone, txnId, amount);

        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(PaymentProvider.MoovMoney, payment.Provider);
        Assert.Equal(phone, payment.PhoneNumber);
        Assert.Equal(txnId, payment.TransactionId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(PaymentStatus.Initiated, payment.Status);
        Assert.Null(payment.ConfirmedAt);
        Assert.Null(payment.FailureReason);
    }

    [Fact]
    public void MarkConfirmed_SetsConfirmedStatusAndTimestamp()
    {
        var payment = Payment.Initiate(OrderId.NewId(), PaymentProvider.OrangeMoney,
            new PhoneNumber("226", "70000001"), TransactionId.From("TXN-001"), Money.From(5000));

        payment.MarkConfirmed();

        Assert.Equal(PaymentStatus.Confirmed, payment.Status);
        Assert.NotNull(payment.ConfirmedAt);
        Assert.Null(payment.FailureReason);
    }

    [Fact]
    public void MarkFailed_SetsFailedStatusAndReason()
    {
        var payment = Payment.Initiate(OrderId.NewId(), PaymentProvider.OrangeMoney,
            new PhoneNumber("226", "70000001"), TransactionId.From("TXN-001"), Money.From(5000));

        payment.MarkFailed("Timeout");

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("Timeout", payment.FailureReason);
    }
}

public class RefundTests
{
    [Fact]
    public void Create_SetsPropertiesWithRequestedStatus()
    {
        var orderId = OrderId.NewId();
        var paymentId = Guid.NewGuid();
        var amount = Money.From(500);

        var refund = Refund.Create(orderId, paymentId, amount, "customer request");

        Assert.Equal(orderId, refund.OrderId);
        Assert.Equal(paymentId, refund.PaymentId);
        Assert.Equal(amount, refund.Amount);
        Assert.Equal("customer request", refund.Reason);
        Assert.Equal(RefundStatus.Requested, refund.Status);
        Assert.Null(refund.ProcessedAt);
    }

    [Fact]
    public void Process_SetsProcessedStatusAndTimestamp()
    {
        var refund = Refund.Create(OrderId.NewId(), Guid.NewGuid(), Money.From(500), "reason");

        refund.Process();

        Assert.Equal(RefundStatus.Processed, refund.Status);
        Assert.NotNull(refund.ProcessedAt);
    }
}
