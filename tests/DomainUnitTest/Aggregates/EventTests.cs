using Domain.Aggregates.Event;
using Domain.Common;
using Domain.DomainEvents.Event;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace DomainUnitTest.Aggregates;

public class EventTests
{
    private static readonly Guid OrganizerId = Guid.NewGuid();
    private static readonly EventSlug Slug = EventSlug.Create("test-event");
    private static readonly Venue Venue = Venue.Create("Salle Prestige", "Ouagadougou", "Rue 10.42");

    private static DateTimeOffset FutureStart => DateTimeOffset.UtcNow.AddDays(30);
    private static DateTimeOffset FutureEnd => DateTimeOffset.UtcNow.AddDays(31);

    private static SalesPeriod DefaultSalesPeriod =>
        SalesPeriod.Create(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(29));

    private static TicketCategory MakeCategory(EventId eventId, int quota = 100, bool isActive = true) =>
        TicketCategory.Create(eventId, "Standard", Money.From(1000), quota, FeePolicy.BuyerPays, isActive);

    private static Event CreateValidEvent(IEnumerable<TicketCategory>? categories = null)
    {
        var eventId = EventId.NewId();
        var cats = categories ?? new[] { MakeCategory(eventId) };
        return Event.Create(OrganizerId, "Concert", "A great event", Slug, Venue,
            FutureStart, FutureEnd, DefaultSalesPeriod, cats);
    }

    // --- Create ---

    [Fact]
    public void Create_WithValidData_SetsPropertiesAndDraftStatus()
    {
        var ev = CreateValidEvent();

        Assert.Equal(OrganizerId, ev.OrganizerId);
        Assert.Equal("Concert", ev.Name);
        Assert.Equal("A great event", ev.Description);
        Assert.Equal(Slug, ev.Slug);
        Assert.Equal(Venue, ev.Venue);
        Assert.Equal(EventStatus.Draft, ev.Status);
        Assert.False(ev.IsPublished);
        Assert.Single(ev.Categories);
        Assert.NotNull(ev.Id);
    }

    [Fact]
    public void Create_WithValidData_RaisesEventCreated()
    {
        var ev = CreateValidEvent();

        Assert.Contains(ev.Events, e => e is EventCreated);
    }

    [Fact]
    public void Create_WithPastStartDate_ThrowsBusinessRuleValidationException()
    {
        var pastStart = DateTimeOffset.UtcNow.AddDays(-1);

        Assert.Throws<BusinessRuleValidationException>(() =>
            Event.Create(OrganizerId, "Concert", "desc", Slug, Venue,
                pastStart, FutureEnd, DefaultSalesPeriod,
                new[] { MakeCategory(EventId.NewId()) }));
    }

    [Fact]
    public void Create_CalculatesCapacityFromActiveCategories()
    {
        var eventId = EventId.NewId();
        var active = TicketCategory.Create(eventId, "VIP", Money.From(5000), 50, FeePolicy.BuyerPays, true);
        var inactive = TicketCategory.Create(eventId, "Free", Money.From(0), 200, FeePolicy.BuyerPays, false);

        var ev = Event.Create(OrganizerId, "Concert", "desc", Slug, Venue,
            FutureStart, FutureEnd, DefaultSalesPeriod, new[] { active, inactive });

        Assert.Equal(50, ev.Capacity.Total);
    }

    // --- AddCategory ---

    [Fact]
    public void AddCategory_AddsAndRecalculatesCapacity()
    {
        var ev = CreateValidEvent();
        var initialCapacity = ev.Capacity.Total;

        var newCat = MakeCategory(ev.Id, quota: 50);
        ev.AddCategory(newCat);

        Assert.Equal(2, ev.Categories.Count);
        Assert.Equal(initialCapacity + 50, ev.Capacity.Total);
    }

    [Fact]
    public void AddCategory_RaisesCategoryAddedEvent()
    {
        var ev = CreateValidEvent();
        ev.ClearEvents();

        var newCat = MakeCategory(ev.Id, quota: 25);
        ev.AddCategory(newCat);

        Assert.Contains(ev.Events, e => e is CategoryAdded);
    }

    [Fact]
    public void AddCategory_NullCategory_ThrowsArgumentNullException()
    {
        var ev = CreateValidEvent();

        Assert.Throws<ArgumentNullException>(() => ev.AddCategory(null!));
    }

    // --- AddPromoCode ---

    [Fact]
    public void AddPromoCode_AddsToCollection()
    {
        var ev = CreateValidEvent();
        var promo = PromoCode.Create(ev.Id, "SAVE10", DiscountType.Percent, 10, 100);

        ev.AddPromoCode(promo);

        Assert.Single(ev.PromoCodes);
    }

    // --- Publish ---

    [Fact]
    public void Publish_WithActiveCategory_ChangesStatusToPublished()
    {
        var ev = CreateValidEvent();
        ev.Publish();

        Assert.Equal(EventStatus.Published, ev.Status);
        Assert.True(ev.IsPublished);
    }

    [Fact]
    public void Publish_RaisesEventPublished()
    {
        var ev = CreateValidEvent();
        ev.ClearEvents();
        ev.Publish();

        Assert.Contains(ev.Events, e => e is EventPublished);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_IsIdempotent()
    {
        var ev = CreateValidEvent();
        ev.Publish();
        ev.ClearEvents();

        ev.Publish();

        Assert.True(ev.IsPublished);
        Assert.Empty(ev.Events);
    }

    [Fact]
    public void Publish_WithNoActiveCategories_ThrowsBusinessRuleValidationException()
    {
        var eventId = EventId.NewId();
        var inactive = TicketCategory.Create(eventId, "Standard", Money.From(1000), 100, FeePolicy.BuyerPays, false);

        // Cannot create event with only inactive categories directly (capacity=0),
        // so create with active, then deactivate
        var ev = CreateValidEvent();
        foreach (var cat in ev.Categories) cat.Deactivate();

        Assert.Throws<BusinessRuleValidationException>(() => ev.Publish());
    }

    // --- Unpublish ---

    [Fact]
    public void Unpublish_WhenPublished_ChangesToDraft()
    {
        var ev = CreateValidEvent();
        ev.Publish();
        ev.ClearEvents();

        ev.Unpublish();

        Assert.Equal(EventStatus.Draft, ev.Status);
        Assert.Contains(ev.Events, e => e is EventUnpublished);
    }

    [Fact]
    public void Unpublish_WhenNotPublished_IsNoOp()
    {
        var ev = CreateValidEvent();
        ev.ClearEvents();

        ev.Unpublish();

        Assert.Equal(EventStatus.Draft, ev.Status);
        Assert.Empty(ev.Events);
    }

    // --- Duplicate ---

    [Fact]
    public void Duplicate_CreatesNewEventWithNewParams()
    {
        var ev = CreateValidEvent();
        var newSlug = EventSlug.Create("dup-event");
        var newSales = SalesPeriod.Create(DateTimeOffset.UtcNow.AddDays(2), DateTimeOffset.UtcNow.AddDays(28));
        var newStart = DateTimeOffset.UtcNow.AddDays(60);
        var newEnd = DateTimeOffset.UtcNow.AddDays(61);

        var dup = ev.Duplicate(newSlug, newSales, newStart, newEnd);

        Assert.NotEqual(ev.Id, dup.Id);
        Assert.Equal(newSlug, dup.Slug);
        Assert.Equal(newStart, dup.StartDate);
        Assert.Equal(newEnd, dup.EndDate);
        Assert.Equal(ev.Categories.Count, dup.Categories.Count);
        Assert.Equal(EventStatus.Draft, dup.Status);
    }

    [Fact]
    public void Duplicate_RaisesEventDuplicated()
    {
        var ev = CreateValidEvent();
        var dup = ev.Duplicate(
            EventSlug.Create("dup-slug"),
            SalesPeriod.Create(DateTimeOffset.UtcNow.AddDays(2), DateTimeOffset.UtcNow.AddDays(28)),
            DateTimeOffset.UtcNow.AddDays(60),
            DateTimeOffset.UtcNow.AddDays(61));

        Assert.Contains(dup.Events, e => e is EventDuplicated);
    }

    // --- CloseSalesIfExpired ---

    [Fact]
    public void CloseSalesIfExpired_WhenSalesPeriodEnded_ClosesStatus()
    {
        var ev = CreateValidEvent();
        var afterSalesEnd = DefaultSalesPeriod.EndDate.AddMinutes(1);

        ev.CloseSalesIfExpired(afterSalesEnd);

        Assert.Equal(EventStatus.SalesClosed, ev.Status);
    }

    [Fact]
    public void CloseSalesIfExpired_RaisesSalesClosedAutomatically()
    {
        var ev = CreateValidEvent();
        ev.ClearEvents();

        ev.CloseSalesIfExpired(DefaultSalesPeriod.EndDate.AddMinutes(1));

        Assert.Contains(ev.Events, e => e is SalesClosedAutomatically);
    }

    [Fact]
    public void CloseSalesIfExpired_WhenAlreadyClosed_IsIdempotent()
    {
        var ev = CreateValidEvent();
        ev.CloseSalesIfExpired(DefaultSalesPeriod.EndDate.AddMinutes(1));
        ev.ClearEvents();

        ev.CloseSalesIfExpired(DefaultSalesPeriod.EndDate.AddMinutes(10));

        Assert.Empty(ev.Events);
        Assert.Equal(EventStatus.SalesClosed, ev.Status);
    }

    [Fact]
    public void CloseSalesIfExpired_WhenSalesPeriodNotEnded_NoChange()
    {
        var ev = CreateValidEvent();
        ev.ClearEvents();

        ev.CloseSalesIfExpired(DefaultSalesPeriod.StartDate);

        Assert.Equal(EventStatus.Draft, ev.Status);
        Assert.Empty(ev.Events);
    }

    // --- AvailableQuota ---

    [Fact]
    public void AvailableQuota_ReturnsCapacityTotalMinusUsedCount()
    {
        var ev = CreateValidEvent();

        Assert.Equal(ev.Capacity.Total - ev.Capacity.UsedCount, ev.AvailableQuota());
    }
}
