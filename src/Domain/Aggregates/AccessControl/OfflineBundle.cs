using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccessControl
{
    /// <summary>
    /// Lot de données hors ligne contenant les payloads QR nécessaires au contrôle d'accès sans connexion.
    /// </summary>
    public sealed class OfflineBundle : Entity<Guid>
    {
        public Guid EventId { get; private set; }
        public IReadOnlyCollection<string> Tickets { get; private set; } = [];
        public DateTimeOffset GeneratedAt { get; private set; }
        public string Signature { get; private set; } = null!;
        public DateTimeOffset ExpiresAt { get; private set; }

        // EF
        private OfflineBundle() { }

        private OfflineBundle(Guid id, Guid eventId, IEnumerable<string> tickets, string signature, DateTimeOffset generatedAt, DateTimeOffset expiresAt)
            : base(id)
        {
            Guard.Against.Null(eventId, nameof(eventId));
            Guard.Against.NullOrEmpty(signature, nameof(signature));

            EventId = eventId;
            Tickets = tickets.ToList().AsReadOnly();
            Signature = signature;
            GeneratedAt = generatedAt;
            ExpiresAt = expiresAt;
        }

        public static OfflineBundle Create(Guid eventId, IEnumerable<string> tickets, string signature, DateTimeOffset now, TimeSpan validity)
        {
            return new OfflineBundle(Guid.NewGuid(), eventId, tickets, signature, now, now.Add(validity));
        }

        public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

        public bool Validate(DateTimeOffset now)
        {
            return !IsExpired(now) && !string.IsNullOrWhiteSpace(Signature);
        }
    }
}
