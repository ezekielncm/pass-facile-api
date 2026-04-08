using Domain.Common;

namespace Domain.Aggregates.AccessControl
{
    /// <summary>
    /// Représente l'affectation d'un agent à un événement pour le contrôle d'accès.
    /// </summary>
    public sealed class AgentAssignment : Entity<Guid>
    {
        public Guid EventId { get; private set; }
        public Guid AgentId { get; private set; }
        public DateTimeOffset AssignedAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }
        public bool IsActive { get; private set; }

        // EF
        private AgentAssignment() { }

        private AgentAssignment(Guid id, Guid eventId, Guid agentId, DateTimeOffset assignedAt)
            : base(id)
        {
            Guard.Against.Null(eventId, nameof(eventId));
            Guard.Against.Null(agentId, nameof(agentId));

            EventId = eventId;
            AgentId = agentId;
            AssignedAt = assignedAt;
            IsActive = true;
        }

        public static AgentAssignment Create(Guid eventId, Guid agentId, DateTimeOffset now)
        {
            return new AgentAssignment(Guid.NewGuid(), eventId, agentId, now);
        }

        public void Revoke(DateTimeOffset now)
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            RevokedAt = now;
        }
    }
}
