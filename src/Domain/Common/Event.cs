namespace Domain.Common
{
    public interface IEvent
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
    public abstract class Event : IEvent
    {
        public DateTime OccurredAt { get; init; }
        public Guid EventId { get; init; }

        protected Event()
        {
            OccurredAt = DateTime.UtcNow;
            EventId = Guid.NewGuid();
        }

        protected Event(DateTime occurredAt)
        {
            OccurredAt = occurredAt;
            EventId = Guid.NewGuid();
        }
    }
}