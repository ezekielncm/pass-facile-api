using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.Event
{
    public sealed class TicketCategory : Entity<Guid>
    {
        public EventId EventId { get; private set; }
        public string Name { get; private set; } = null!;
        public Money Price { get; private set; } = Money.From(0);
        public int Quota { get; private set; }
        public int SoldCount { get; private set; }
        public FeePolicy FeePolicy { get; private set; }
        public bool IsActive { get; private set; }
        public string? Description { get; private set; }

        // EF Core
        private TicketCategory() { }

        private TicketCategory(
            Guid id,
            EventId eventId,
            string name,
            Money price,
            int quota,
            FeePolicy feePolicy,
            bool isActive,
            string? description)
            : base(id)
        {
            Guard.Against.Null(eventId, nameof(eventId));
            Guard.Against.NullOrEmpty(name, nameof(name));

            if (price.Amount < 0)
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
            FeePolicy = feePolicy;
            IsActive = isActive;
            Description = description;
        }

        public static TicketCategory Create(
            EventId eventId,
            string name,
            Money price,
            int quota,
            FeePolicy feePolicy = FeePolicy.BuyerPays,
            bool isActive = true,
            string? description = null)
        {
            return new TicketCategory(Guid.NewGuid(), eventId, name, price, quota, feePolicy, isActive, description);
        }

        internal TicketCategory CloneFor(EventId newEventId)
        {
            return Create(newEventId, Name, Price, Quota, FeePolicy, IsActive, Description);
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;

        public int RemainingQuota() => Quota - SoldCount;

        public Money ComputeFinalPrice()
        {
            // Retourne le prix tel quel ; la logique de fee est gérée par la politique
            return Price;
        }
    }
}
