using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.Event
{
    public sealed class PromoCode : Entity<Guid>
    {
        public EventId EventId { get; private set; }
        public string Code { get; private set; } = null!;
        public DiscountType DiscountType { get; private set; }
        public decimal Value { get; private set; }
        public int MaxUses { get; private set; }
        public int UsedCount { get; private set; }
        public DateTimeOffset? ExpiresAt { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core
        private PromoCode() { }

        private PromoCode(
            Guid id,
            EventId eventId,
            string code,
            DiscountType discountType,
            decimal value,
            int maxUses,
            DateTimeOffset? expiresAt,
            bool isActive)
            : base(id)
        {
            Guard.Against.Null(eventId, nameof(eventId));
            Guard.Against.NullOrEmpty(code, nameof(code));

            if (value <= 0)
            {
                throw new BusinessRuleValidationException("PromoCode.InvalidDiscount",
                    "La valeur de réduction doit être strictement positive.");
            }

            EventId = eventId;
            Code = code.Trim().ToUpperInvariant();
            DiscountType = discountType;
            Value = value;
            MaxUses = maxUses;
            ExpiresAt = expiresAt;
            IsActive = isActive;
        }

        public static PromoCode Create(
            EventId eventId,
            string code,
            DiscountType discountType,
            decimal value,
            int maxUses,
            DateTimeOffset? expiresAt = null)
        {
            return new PromoCode(Guid.NewGuid(), eventId, code, discountType, value, maxUses, expiresAt, true);
        }

        public bool IsValid(DateTimeOffset now)
        {
            if (!IsActive) return false;
            if (UsedCount >= MaxUses) return false;
            if (ExpiresAt is null) return true;
            return now <= ExpiresAt.Value;
        }

        public Money Apply(Money price)
        {
            if (DiscountType == Enums.DiscountType.Fixed)
            {
                var discounted = price.Amount - Value;
                return Money.From(discounted < 0 ? 0 : discounted, price.Currency);
            }
            else
            {
                var discounted = price.Amount * (1 - Value / 100);
                return Money.From(discounted < 0 ? 0 : discounted, price.Currency);
            }
        }

        public void IncrementUsage()
        {
            if (UsedCount >= MaxUses)
            {
                throw new BusinessRuleValidationException("PromoCode.MaxUsesReached",
                    "Le nombre maximum d'utilisations de ce code promo a été atteint.");
            }

            UsedCount++;
        }

        public void Deactivate() => IsActive = false;
    }
}
