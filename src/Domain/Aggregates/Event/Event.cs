using Domain.Common;
using Domain.DomainEvents.Event;
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
        public string Name { get; private set; } = null!;
        public string Description { get; private set; }
        public EventSlug Slug { get; private set; } = null!;
        public Venue Venue { get; private set; } = null!;
        public SalesPeriod SalesPeriod { get; private set; } = null!;
        public Capacity Capacity { get; private set; } = Capacity.From(0);
        public DateTimeOffset EventDate { get; private set; }
        public string? CoverImageUrl { get; private set; }
        public bool IsPublished { get; private set; }
        public bool SalesClosed { get; private set; }

        public IReadOnlyCollection<TicketCategory> Categories => _categories.AsReadOnly();
        public IReadOnlyCollection<PromoCode> PromoCodes => _promoCodes.AsReadOnly();

        // EF Core
        private Event() { }

        private Event(
            EventId id,
            EventSlug slug,
            Venue venue,
            SalesPeriod salesPeriod,
            DateTimeOffset eventDate,
            IEnumerable<TicketCategory> categories)
            : base(id)
        {
            if (eventDate <= DateTimeOffset.UtcNow)
            {
                throw new BusinessRuleValidationException("Event.DateInPast",
                    "La date de l'événement doit être dans le futur lors de la création.");
            }

            Id = id;
            Slug = slug;
            Venue = venue;
            SalesPeriod = salesPeriod;
            EventDate = eventDate;

            _categories.AddRange(categories);
            Capacity = RecalculateCapacity();

            RaiseEvent(new EventCreated(Id));
        }

        public static Event Create(

            EventSlug slug,
            Venue venue,
            SalesPeriod salesPeriod,
            DateTimeOffset eventDate,
            IEnumerable<TicketCategory> categories)
        {
            var id = EventId.NewId();
            var cats = categories.ToList();
            return new Event(id, slug, venue, salesPeriod, eventDate, cats);
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
            if (IsPublished)
            {
                return;
            }

            if (!_categories.Any(c => c.IsActive))
            {
                throw new BusinessRuleValidationException("Event.NoActiveCategory",
                    "Un événement ne peut être publié que s'il possède au moins une catégorie active.");
            }

            IsPublished = true;
            RaiseEvent(new EventPublished(Id));
        }

        public void Unpublish()
        {
            if (!IsPublished)
            {
                return;
            }

            IsPublished = false;
            RaiseEvent(new EventUnpublished(Id));
        }

        public Event Duplicate(EventSlug newSlug, SalesPeriod newSalesPeriod, DateTimeOffset newEventDate)
        {
            var duplicate = Create(
                newSlug,
                Venue,
                newSalesPeriod,
                newEventDate,
                Array.Empty<TicketCategory>());

            foreach (var category in _categories)
            {
                duplicate.AddCategory(category.CloneFor(duplicate.Id));
            }

            duplicate.RaiseEvent(new EventDuplicated(Id, duplicate.Id));
            return duplicate;
        }

        public void CloseSalesIfNeeded(DateTimeOffset now)
        {
            if (SalesClosed)
            {
                return;
            }

            if (SalesPeriod.HasEnded(now))
            {
                SalesClosed = true;
                RaiseEvent(new SalesClosedAutomatically(Id));
            }
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
