using Domain.Aggregates.Event;
using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace DomainUnitTest.Aggregates;

public class PromoCodeTests
{
    private static readonly EventId ValidEventId = EventId.NewId();

    private static PromoCode CreateValid(
        DiscountType type = DiscountType.Fixed,
        decimal value = 500,
        int maxUses = 10,
        DateTimeOffset? expiresAt = null) =>
        PromoCode.Create(ValidEventId, "SAVE50", type, value, maxUses, expiresAt);

    // --- Create ---

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var promo = CreateValid();

        Assert.Equal(ValidEventId, promo.EventId);
        Assert.Equal("SAVE50", promo.Code);
        Assert.Equal(DiscountType.Fixed, promo.DiscountType);
        Assert.Equal(500m, promo.Value);
        Assert.Equal(10, promo.MaxUses);
        Assert.Equal(0, promo.UsedCount);
        Assert.True(promo.IsActive);
        Assert.Null(promo.ExpiresAt);
    }

    [Fact]
    public void Create_NormalizesCodeToUppercase()
    {
        var promo = PromoCode.Create(ValidEventId, "  hello  ", DiscountType.Fixed, 100, 5);

        Assert.Equal("HELLO", promo.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullCode_ThrowsArgumentException(string? code)
    {
        Assert.Throws<ArgumentException>(() =>
            PromoCode.Create(ValidEventId, code!, DiscountType.Fixed, 100, 5));
    }

    [Fact]
    public void Create_WithNullEventId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PromoCode.Create(null!, "CODE", DiscountType.Fixed, 100, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithNonPositiveValue_ThrowsBusinessRuleValidationException(decimal value)
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            PromoCode.Create(ValidEventId, "CODE", DiscountType.Fixed, value, 5));
    }

    // --- IsValid ---

    [Fact]
    public void IsValid_WhenActiveAndUnderMaxUses_ReturnsTrue()
    {
        var promo = CreateValid();

        Assert.True(promo.IsValid(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsValid_WhenInactive_ReturnsFalse()
    {
        var promo = CreateValid();
        promo.Deactivate();

        Assert.False(promo.IsValid(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsValid_WhenMaxUsesReached_ReturnsFalse()
    {
        var promo = CreateValid(maxUses: 1);
        promo.IncrementUsage();

        Assert.False(promo.IsValid(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsValid_WhenExpired_ReturnsFalse()
    {
        var promo = CreateValid(expiresAt: DateTimeOffset.UtcNow.AddHours(-1));

        Assert.False(promo.IsValid(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsValid_WhenNoExpiresAt_ReturnsTrue()
    {
        var promo = CreateValid(expiresAt: null);

        Assert.True(promo.IsValid(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsValid_WhenBeforeExpiry_ReturnsTrue()
    {
        var promo = CreateValid(expiresAt: DateTimeOffset.UtcNow.AddHours(1));

        Assert.True(promo.IsValid(DateTimeOffset.UtcNow));
    }

    // --- Apply ---

    [Fact]
    public void Apply_FixedDiscount_SubtractsValueFromPrice()
    {
        var promo = CreateValid(type: DiscountType.Fixed, value: 300);
        var price = Money.From(1000);

        var result = promo.Apply(price);

        Assert.Equal(700m, result.Amount);
    }

    [Fact]
    public void Apply_FixedDiscount_FloorsAtZero()
    {
        var promo = CreateValid(type: DiscountType.Fixed, value: 2000);
        var price = Money.From(1000);

        var result = promo.Apply(price);

        Assert.Equal(0m, result.Amount);
    }

    [Fact]
    public void Apply_PercentageDiscount_AppliesPercentage()
    {
        var promo = CreateValid(type: DiscountType.Percent, value: 20);
        var price = Money.From(1000);

        var result = promo.Apply(price);

        Assert.Equal(800m, result.Amount);
    }

    [Fact]
    public void Apply_PercentageDiscount_100Percent_ReturnsZero()
    {
        var promo = CreateValid(type: DiscountType.Percent, value: 100);
        var price = Money.From(1000);

        var result = promo.Apply(price);

        Assert.Equal(0m, result.Amount);
    }

    // --- IncrementUsage ---

    [Fact]
    public void IncrementUsage_IncrementsUsedCount()
    {
        var promo = CreateValid(maxUses: 5);

        promo.IncrementUsage();

        Assert.Equal(1, promo.UsedCount);
    }

    [Fact]
    public void IncrementUsage_WhenMaxReached_ThrowsBusinessRuleValidationException()
    {
        var promo = CreateValid(maxUses: 1);
        promo.IncrementUsage();

        Assert.Throws<BusinessRuleValidationException>(() => promo.IncrementUsage());
    }

    // --- Deactivate ---

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var promo = CreateValid();

        promo.Deactivate();

        Assert.False(promo.IsActive);
    }
}
