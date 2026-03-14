namespace Domain.Common
{
    public interface IDomainEvent
    {
        /// <summary>
        /// When the event occurred.
        /// </summary>
        DateTime OccurredAt { get; }

        /// <summary>
        /// Unique identifier for this event instance.
        /// </summary>
        Guid EventId { get; }
    }

    /// <summary>
    /// Base record for domain events (recommended approach with C# records).
    /// </summary>
    public abstract record DomainEvent : IDomainEvent
    {
        public DateTime OccurredAt { get; init; }
        public Guid EventId { get; init; }

        protected DomainEvent()
        {
            OccurredAt = DateTime.UtcNow;
            EventId = Guid.NewGuid();
        }

        protected DomainEvent(DateTime occurredAt)
        {
            OccurredAt = occurredAt;
            EventId = Guid.NewGuid();
        }
    }
}