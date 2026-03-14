using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.Event
{
    public sealed class PromoCode : Entity<Guid>
    {
        public EventId EventId { get; private set; }
        public string Code { get; private set; } = null!;
        public decimal DiscountAmount { get; private set; }
        public DateTimeOffset? ExpiresAt { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core
        private PromoCode() { }

        private PromoCode(
            Guid id,
            EventId eventId,
            string code,
            decimal discountAmount,
            DateTimeOffset? expiresAt,
            bool isActive)
            : base(id)
        {
            Guard.Against.Null(eventId, nameof(eventId));
            Guard.Against.NullOrEmpty(code, nameof(code));

            if (discountAmount <= 0)
            {
                throw new BusinessRuleValidationException("PromoCode.InvalidDiscount",
                    "Le montant de réduction doit être strictement positif.");
            }

            EventId = eventId;
            Code = code.Trim().ToUpperInvariant();
            DiscountAmount = discountAmount;
            ExpiresAt = expiresAt;
            IsActive = isActive;
        }

        public static PromoCode Create(
            EventId eventId,
            string code,
            decimal discountAmount,
            DateTimeOffset? expiresAt = null)
        {
            return new PromoCode(Guid.NewGuid(), eventId, code, discountAmount, expiresAt, true);
        }

        public bool IsValid(DateTimeOffset now)
        {
            if (!IsActive) return false;
            if (ExpiresAt is null) return true;
            return now <= ExpiresAt.Value;
        }

        public void Deactivate() => IsActive = false;
    }
}
