using Domain.Common;
using Domain.DomainEvents.Event;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.Event
{
    /// <summary>
    /// Agrégat racine Event (gestion d'événements).
    /// </summary>
    public sealed class Event : AggregateRoot<EventId>
    {
        private readonly List<TicketCategory> _categories = [];
        private readonly List<PromoCode> _promoCodes = [];

        public Guid OrganizerId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Description { get; private set; }
        public EventSlug Slug { get; private set; } = null!;
        public string? CoverImageUrl { get; private set; }
        public Venue Venue { get; private set; } = null!;
        public DateTimeOffset StartDate { get; private set; }
        public DateTimeOffset EndDate { get; private set; }
        public SalesPeriod SalesPeriod { get; private set; } = null!;
        public Capacity Capacity { get; private set; } = Capacity.From(0);
        public EventStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public IReadOnlyCollection<TicketCategory> Categories => _categories.AsReadOnly();
        public IReadOnlyCollection<PromoCode> PromoCodes => _promoCodes.AsReadOnly();

        /// <summary>
        /// Convenience property derived from Status.
        /// </summary>
        public bool IsPublished => Status == EventStatus.Published;

        // EF Core
        private Event() { }

        private Event(
            EventId id,
            Guid organizerId,
            string name,
            string description,
            EventSlug slug,
            Venue venue,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            SalesPeriod salesPeriod,
            IEnumerable<TicketCategory> categories)
            : base(id)
        {
            if (startDate <= DateTimeOffset.UtcNow)
            {
                throw new BusinessRuleValidationException("Event.DateInPast",
                    "La date de l'événement doit être dans le futur lors de la création.");
            }

            Id = id;
            OrganizerId = organizerId;
            Name = name;
            Description = description;
            Slug = slug;
            Venue = venue;
            StartDate = startDate;
            EndDate = endDate;
            SalesPeriod = salesPeriod;
            Status = EventStatus.Draft;
            CreatedAt = DateTimeOffset.UtcNow;

            _categories.AddRange(categories);
            Capacity = RecalculateCapacity();

            RaiseEvent(new EventCreated(Id));
        }

        public static Event Create(
            Guid organizerId,
            string name,
            string description,
            EventSlug slug,
            Venue venue,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            SalesPeriod salesPeriod,
            IEnumerable<TicketCategory> categories)
        {
            var id = EventId.NewId();
            var cats = categories.ToList();
            return new Event(id, organizerId, name, description, slug, venue, startDate, endDate, salesPeriod, cats);
        }

        public void AddCategory(TicketCategory category)
        {
            Guard.Against.Null(category, nameof(category));

            _categories.Add(category);
            Capacity = RecalculateCapacity();

            RaiseEvent(new CategoryAdded(Id, category.Id));
        }

        public void Publish()
        {
            if (Status == EventStatus.Published)
            {
                return;
            }

            if (!_categories.Any(c => c.IsActive))
            {
                throw new BusinessRuleValidationException("Event.NoActiveCategory",
                    "Un événement ne peut être publié que s'il possède au moins une catégorie active.");
            }

            Status = EventStatus.Published;
            RaiseEvent(new EventPublished(Id));
        }

        public void Unpublish()
        {
            if (Status != EventStatus.Published)
            {
                return;
            }

            Status = EventStatus.Draft;
            RaiseEvent(new EventUnpublished(Id));
        }

        public Event Duplicate(EventSlug newSlug, SalesPeriod newSalesPeriod, DateTimeOffset newStartDate, DateTimeOffset newEndDate)
        {
            var duplicate = Create(
                OrganizerId,
                Name,
                Description,
                newSlug,
                Venue,
                newStartDate,
                newEndDate,
                newSalesPeriod,
                Array.Empty<TicketCategory>());

            foreach (var category in _categories)
            {
                duplicate.AddCategory(category.CloneFor(duplicate.Id));
            }

            duplicate.RaiseEvent(new EventDuplicated(Id, duplicate.Id));
            return duplicate;
        }

        public void CloseSalesIfExpired(DateTimeOffset now)
        {
            if (Status == EventStatus.SalesClosed)
            {
                return;
            }

            if (SalesPeriod.HasEnded(now))
            {
                Status = EventStatus.SalesClosed;
                RaiseEvent(new SalesClosedAutomatically(Id));
            }
        }

        public int AvailableQuota()
        {
            return Capacity.Total - Capacity.UsedCount;
        }

        private Capacity RecalculateCapacity()
        {
            var total = _categories
                .Where(c => c.IsActive)
                .Sum(c => c.Quota);

            return Capacity.From(total);
        }
    }
}
