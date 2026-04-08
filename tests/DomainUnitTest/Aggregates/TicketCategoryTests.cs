using Domain.Aggregates.Event;
using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace DomainUnitTest.Aggregates;

public class TicketCategoryTests
{
    private static readonly EventId ValidEventId = EventId.NewId();

    private static TicketCategory CreateValid(
        int quota = 100,
        bool isActive = true,
        decimal priceAmount = 1000) =>
        TicketCategory.Create(ValidEventId, "Standard", Money.From(priceAmount), quota,
            FeePolicy.BuyerPays, isActive, "A ticket category");

    // --- Create ---

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var cat = CreateValid();

        Assert.Equal(ValidEventId, cat.EventId);
        Assert.Equal("Standard", cat.Name);
        Assert.Equal(Money.From(1000), cat.Price);
        Assert.Equal(100, cat.Quota);
        Assert.Equal(0, cat.SoldCount);
        Assert.Equal(FeePolicy.BuyerPays, cat.FeePolicy);
        Assert.True(cat.IsActive);
        Assert.Equal("A ticket category", cat.Description);
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsBusinessRuleValidationException()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            TicketCategory.Create(ValidEventId, "Cat", Money.From(-1), 10));
    }

    [Fact]
    public void Create_WithZeroPrice_Succeeds()
    {
        var cat = TicketCategory.Create(ValidEventId, "Free", Money.From(0), 50);

        Assert.Equal(0m, cat.Price.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeQuota_ThrowsBusinessRuleValidationException(int quota)
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            TicketCategory.Create(ValidEventId, "Cat", Money.From(500), quota));
    }

    [Fact]
    public void Create_WithNullEventId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TicketCategory.Create(null!, "Cat", Money.From(500), 10));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            TicketCategory.Create(ValidEventId, name!, Money.From(500), 10));
    }

    [Fact]
    public void Create_TrimsName()
    {
        var cat = TicketCategory.Create(ValidEventId, "  VIP  ", Money.From(5000), 20);

        Assert.Equal("VIP", cat.Name);
    }

    // --- Activate / Deactivate ---

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        var cat = CreateValid(isActive: false);

        cat.Activate();

        Assert.True(cat.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var cat = CreateValid(isActive: true);

        cat.Deactivate();

        Assert.False(cat.IsActive);
    }

    // --- RemainingQuota ---

    [Fact]
    public void RemainingQuota_ReturnsQuotaMinusSoldCount()
    {
        var cat = CreateValid(quota: 100);

        // SoldCount starts at 0
        Assert.Equal(100, cat.RemainingQuota());
    }

    // --- ComputeFinalPrice ---

    [Fact]
    public void ComputeFinalPrice_ReturnsPrice()
    {
        var cat = CreateValid(priceAmount: 2500);

        Assert.Equal(Money.From(2500), cat.ComputeFinalPrice());
    }

    // --- Default fee policy ---

    [Fact]
    public void Create_DefaultFeePolicy_IsBuyerPays()
    {
        var cat = TicketCategory.Create(ValidEventId, "Standard", Money.From(1000), 50);

        Assert.Equal(FeePolicy.BuyerPays, cat.FeePolicy);
    }
}
