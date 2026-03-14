using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.Event
{
    public sealed class TicketCategory : Entity<Guid>
    {
        public EventId EventId { get; private set; }
        public string Name { get; private set; } = null!;
        public decimal Price { get; private set; }
        public int Quota { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core
        private TicketCategory() { }

        private TicketCategory(
            Guid id,
            EventId eventId,
            string name,
            decimal price,
            int quota,
            bool isActive)
            : base(id)
        {
            Guard.Against.Null(eventId, nameof(eventId));
            Guard.Against.NullOrEmpty(name, nameof(name));

            if (price < 0)
            {
                throw new BusinessRuleValidationException("TicketCategory.NegativePrice",
                    "Le prix d'une catégorie ne peut pas être négatif.");
            }

            if (quota <= 0)
            {
                throw new BusinessRuleValidationException("TicketCategory.InvalidQuota",
                    "Le quota d'une catégorie doit être strictement positif.");
            }

            EventId = eventId;
            Name = name.Trim();
            Price = price;
            Quota = quota;
            IsActive = isActive;
        }

        public static TicketCategory Create(
            EventId eventId,
            string name,
            decimal price,
            int quota,
            bool isActive = true)
        {
            return new TicketCategory(Guid.NewGuid(), eventId, name, price, quota, isActive);
        }

        internal TicketCategory CloneFor(EventId newEventId)
        {
            return Create(newEventId, Name, Price, Quota, IsActive);
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;
    }
}
