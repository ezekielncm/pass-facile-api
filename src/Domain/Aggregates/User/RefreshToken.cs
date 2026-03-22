using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.User
{
    /// <summary>
    /// Jeton de rafraîchissement permettant de renouveler l'accès sans ré-authentification.
    /// </summary>
    public sealed class RefreshToken : Entity<Guid>
    {
        public string Token { get; private set; } = null!;
        public UserId UserId { get; private set; } = null!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }

        // EF
        private RefreshToken() { }

        private RefreshToken(Guid id, string token, UserId userId, DateTimeOffset expiresAt)
            : base(id)
        {
            Guard.Against.NullOrEmpty(token, nameof(token));
            Guard.Against.Null(userId, nameof(userId));

            Token = token;
            UserId = userId;
            ExpiresAt = expiresAt;
        }

        public static RefreshToken Create(string token, UserId userId, DateTimeOffset expiresAt)
        {
            return new RefreshToken(Guid.NewGuid(), token, userId, expiresAt);
        }

        public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

        public RefreshToken Rotate(string newToken, DateTimeOffset newExpiresAt)
        {
            Guard.Against.NullOrEmpty(newToken, nameof(newToken));

            Revoke();
            return Create(newToken, UserId, newExpiresAt);
        }

        public void Revoke()
        {
            IsRevoked = true;
        }
    }
}
