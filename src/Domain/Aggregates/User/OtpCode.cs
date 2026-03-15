using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Aggregates.User
{
    /// <summary>
    /// Représentation métier d'un OTP associé à un numéro de téléphone.
    /// (L'implémentation actuelle côté infra utilise un cache en mémoire,
    /// mais ce modèle permet de raisonner sur les règles de validité).
    /// </summary>
    public sealed class OTPCode : Entity<Guid>
    {
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public string Code { get; private set; } = null!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public bool Used { get; private set; }

        private const int DefaultExpiryMinutes = 5;

        // EF
        private OTPCode() { }

        private OTPCode(Guid id, PhoneNumber phoneNumber, string code, DateTimeOffset createdAt, DateTimeOffset expiresAt)
            : base(id)
        {
            Guard.Against.Null(phoneNumber, nameof(phoneNumber));
            Guard.Against.NullOrEmpty(code, nameof(code));

            PhoneNumber = phoneNumber;
            Code = code.Trim();
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        public static OTPCode Issue(PhoneNumber phoneNumber, string code, DateTimeOffset now)
        {
            var expiresAt = now.AddMinutes(DefaultExpiryMinutes);
            return new OTPCode(Guid.NewGuid(), phoneNumber, code, now, expiresAt);
        }

        public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

        public bool CanBeUsed(DateTimeOffset now) => !Used && !IsExpired(now);

        public void MarkUsed(DateTimeOffset now)
        {
            if (!CanBeUsed(now))
            {
                throw new BusinessRuleValidationException(
                    "OTP_INVALID",
                    "Le code OTP est expiré ou déjà utilisé.");
            }

            Used = true;
        }
    }
}

