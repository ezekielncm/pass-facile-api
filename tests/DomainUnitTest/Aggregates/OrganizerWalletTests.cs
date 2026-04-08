using Domain.Aggregates.Finance;
using Domain.Common;
using Domain.DomainEvents.Finance;
using Domain.ValueObjects;

namespace DomainUnitTest.Aggregates;

public class OrganizerWalletTests
{
    private static readonly Guid OrganizerId = Guid.NewGuid();

    private static OrganizerWallet CreateValid(string currency = "XOF") =>
        OrganizerWallet.Create(OrganizerId, currency);

    // --- Create ---

    [Fact]
    public void Create_SetsPropertiesWithZeroBalance()
    {
        var wallet = CreateValid();

        Assert.Equal(OrganizerId, wallet.OrganizerId);
        Assert.Equal("XOF", wallet.Currency);
        Assert.Equal(0m, wallet.Balance.Available.Amount);
        Assert.Equal(0m, wallet.Balance.Pending.Amount);
        Assert.Empty(wallet.Withdrawals);
        Assert.Empty(wallet.Fees);
    }

    [Fact]
    public void Create_WithCustomCurrency_SetsCurrency()
    {
        var wallet = CreateValid("EUR");

        Assert.Equal("EUR", wallet.Currency);
    }

    // --- Credit ---

    [Fact]
    public void Credit_IncreasesAvailableBalance()
    {
        var wallet = CreateValid();

        wallet.Credit(Money.From(5000));

        Assert.Equal(5000m, wallet.Balance.Available.Amount);
    }

    [Fact]
    public void Credit_MultipleTimes_Accumulates()
    {
        var wallet = CreateValid();

        wallet.Credit(Money.From(1000));
        wallet.Credit(Money.From(2000));

        Assert.Equal(3000m, wallet.Balance.Available.Amount);
    }

    // --- Debit ---

    [Fact]
    public void Debit_DecreasesAvailableBalance()
    {
        var wallet = CreateValid();
        wallet.Credit(Money.From(5000));

        wallet.Debit(Money.From(2000));

        Assert.Equal(3000m, wallet.Balance.Available.Amount);
    }

    [Fact]
    public void Debit_InsufficientFunds_ThrowsBusinessRuleValidationException()
    {
        var wallet = CreateValid();
        wallet.Credit(Money.From(1000));

        Assert.Throws<BusinessRuleValidationException>(() =>
            wallet.Debit(Money.From(1000)));
    }

    // --- ReleasePending ---

    [Fact]
    public void ReleasePending_After24HoursFromEventEnd_AttemptsMoveOfFullPending()
    {
        // Arrange: create wallet with pending balance
        var wallet = CreateValid();
        var now = DateTimeOffset.UtcNow;
        var eventEndFuture = now.AddDays(2);
        wallet.ReceiveRevenue(Guid.NewGuid(), Guid.NewGuid(), Money.From(10000),
            Money.From(500), eventEndFuture, now);

        Assert.True(wallet.Balance.Pending.Amount > 0);

        // Act & Assert: MovePendingToAvailable uses >= guard so exact amount throws
        var pastEventEnd = now.AddDays(-2);
        Assert.Throws<BusinessRuleValidationException>(() =>
            wallet.ReleasePending(Guid.NewGuid(), pastEventEnd, now));
    }

    [Fact]
    public void ReleasePending_Before24HoursFromEventEnd_NoChange()
    {
        var wallet = CreateValid();
        var eventEnd = DateTimeOffset.UtcNow;
        var now = DateTimeOffset.UtcNow.AddHours(23);

        // Manually add pending via ReceiveRevenue
        wallet.ReceiveRevenue(Guid.NewGuid(), Guid.NewGuid(), Money.From(5000),
            Money.From(250), eventEnd.AddDays(5), now);

        var pendingBefore = wallet.Balance.Pending.Amount;

        wallet.ReleasePending(Guid.NewGuid(), eventEnd, now);

        Assert.Equal(pendingBefore, wallet.Balance.Pending.Amount);
    }

    [Fact]
    public void ReleasePending_WhenNoPending_NoChange()
    {
        var wallet = CreateValid();
        var eventEnd = DateTimeOffset.UtcNow.AddDays(-2);

        wallet.ReleasePending(Guid.NewGuid(), eventEnd, DateTimeOffset.UtcNow);

        Assert.Equal(0m, wallet.Balance.Available.Amount);
    }

    // --- ReceiveRevenue ---

    [Fact]
    public void ReceiveRevenue_BeforeEventEnd24h_AddsToPending()
    {
        var wallet = CreateValid();
        var now = DateTimeOffset.UtcNow;
        var eventEnd = now.AddDays(5);

        wallet.ReceiveRevenue(Guid.NewGuid(), Guid.NewGuid(), Money.From(10000),
            Money.From(500), eventEnd, now);

        Assert.Equal(9500m, wallet.Balance.Pending.Amount);
        Assert.Equal(0m, wallet.Balance.Available.Amount);
        Assert.Single(wallet.Fees);
    }

    [Fact]
    public void ReceiveRevenue_AfterEventEnd24h_AddsToAvailable()
    {
        var wallet = CreateValid();
        var eventEnd = DateTimeOffset.UtcNow.AddDays(-2);
        var now = DateTimeOffset.UtcNow;

        wallet.ReceiveRevenue(Guid.NewGuid(), Guid.NewGuid(), Money.From(10000),
            Money.From(500), eventEnd, now);

        Assert.Equal(9500m, wallet.Balance.Available.Amount);
    }

    [Fact]
    public void ReceiveRevenue_RaisesRevenueReceived()
    {
        var wallet = CreateValid();
        wallet.ClearEvents();

        wallet.ReceiveRevenue(Guid.NewGuid(), Guid.NewGuid(), Money.From(5000),
            Money.From(250), DateTimeOffset.UtcNow.AddDays(5), DateTimeOffset.UtcNow);

        Assert.Contains(wallet.Events, e => e is RevenueReceived);
    }

    [Fact]
    public void ReceiveRevenue_CreatesFeeTransaction()
    {
        var wallet = CreateValid();
        var orderId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        wallet.ReceiveRevenue(orderId, eventId, Money.From(10000),
            Money.From(500), DateTimeOffset.UtcNow.AddDays(5), DateTimeOffset.UtcNow);

        var fee = wallet.Fees.Single();
        Assert.Equal(orderId, fee.OrderId);
        Assert.Equal(eventId, fee.EventId);
        Assert.Equal(10000m, fee.GrossAmount.Amount);
        Assert.Equal(500m, fee.PlatformFee.Amount);
        Assert.Equal(9500m, fee.NetAmount.Amount);
    }

    // --- RequestWithdrawal ---

    [Fact]
    public void RequestWithdrawal_DeductsBalanceAndCreatesWithdrawal()
    {
        var wallet = CreateValid();
        wallet.Credit(Money.From(10000));

        var withdrawal = wallet.RequestWithdrawal(Money.From(5000));

        Assert.NotNull(withdrawal);
        Assert.Equal(5000m, withdrawal.Amount.Amount);
        Assert.Equal(WithdrawalStatus.Requested, withdrawal.Status);
        Assert.Single(wallet.Withdrawals);
        Assert.True(wallet.Balance.Available.Amount < 10000m);
    }

    [Fact]
    public void RequestWithdrawal_RaisesWithdrawalRequested()
    {
        var wallet = CreateValid();
        wallet.Credit(Money.From(10000));
        wallet.ClearEvents();

        wallet.RequestWithdrawal(Money.From(3000));

        Assert.Contains(wallet.Events, e => e is WithdrawalRequested);
    }

    [Fact]
    public void RequestWithdrawal_InsufficientFunds_ThrowsBusinessRuleValidationException()
    {
        var wallet = CreateValid();
        wallet.Credit(Money.From(1000));

        Assert.Throws<BusinessRuleValidationException>(() =>
            wallet.RequestWithdrawal(Money.From(1000)));
    }

    // --- MarkWithdrawalProcessed ---

    [Fact]
    public void MarkWithdrawalProcessed_SetsCompletedStatusAndRaisesEvent()
    {
        var wallet = CreateValid();
        wallet.Credit(Money.From(10000));
        var withdrawal = wallet.RequestWithdrawal(Money.From(5000));
        wallet.ClearEvents();

        wallet.MarkWithdrawalProcessed(withdrawal.Id);

        Assert.Equal(WithdrawalStatus.Completed, withdrawal.Status);
        Assert.NotNull(withdrawal.ProcessedAt);
        Assert.Contains(wallet.Events, e => e is WithdrawalProcessed);
    }

    [Fact]
    public void MarkWithdrawalProcessed_WithUnknownId_IsNoOp()
    {
        var wallet = CreateValid();
        wallet.ClearEvents();

        wallet.MarkWithdrawalProcessed(Guid.NewGuid());

        Assert.Empty(wallet.Events);
    }

    // --- MarkWithdrawalFailed ---

    [Fact]
    public void MarkWithdrawalFailed_SetsFailedStatusAndRaisesEvent()
    {
        var wallet = CreateValid();
        wallet.Credit(Money.From(10000));
        var withdrawal = wallet.RequestWithdrawal(Money.From(5000));
        wallet.ClearEvents();

        wallet.MarkWithdrawalFailed(withdrawal.Id);

        Assert.Equal(WithdrawalStatus.Failed, withdrawal.Status);
        Assert.Contains(wallet.Events, e => e is WithdrawalFailed);
    }

    [Fact]
    public void MarkWithdrawalFailed_WithUnknownId_IsNoOp()
    {
        var wallet = CreateValid();
        wallet.ClearEvents();

        wallet.MarkWithdrawalFailed(Guid.NewGuid());

        Assert.Empty(wallet.Events);
    }
}

public class WithdrawalRequestTests
{
    [Fact]
    public void Create_SetsPropertiesWithRequestedStatus()
    {
        var walletId = Guid.NewGuid();

        var withdrawal = WithdrawalRequest.Create(walletId, Money.From(5000));

        Assert.Equal(walletId, withdrawal.OrganizerWalletId);
        Assert.Equal(5000m, withdrawal.Amount.Amount);
        Assert.Equal(WithdrawalStatus.Requested, withdrawal.Status);
        Assert.Null(withdrawal.ProcessedAt);
    }

    [Fact]
    public void MarkProcessed_SetsCompletedStatusAndTimestamp()
    {
        var withdrawal = WithdrawalRequest.Create(Guid.NewGuid(), Money.From(5000));

        withdrawal.MarkProcessed();

        Assert.Equal(WithdrawalStatus.Completed, withdrawal.Status);
        Assert.NotNull(withdrawal.ProcessedAt);
    }

    [Fact]
    public void MarkFailed_SetsFailedStatus()
    {
        var withdrawal = WithdrawalRequest.Create(Guid.NewGuid(), Money.From(5000));

        withdrawal.MarkFailed();

        Assert.Equal(WithdrawalStatus.Failed, withdrawal.Status);
    }
}

public class FeeTransactionTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var walletId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var fee = FeeTransaction.Create(walletId, orderId, eventId,
            Money.From(10000), Money.From(500), Money.From(9500));

        Assert.Equal(walletId, fee.OrganizerWalletId);
        Assert.Equal(orderId, fee.OrderId);
        Assert.Equal(eventId, fee.EventId);
        Assert.Equal(10000m, fee.GrossAmount.Amount);
        Assert.Equal(500m, fee.PlatformFee.Amount);
        Assert.Equal(9500m, fee.NetAmount.Amount);
    }
}
