using Domain.Common;

namespace Domain.Common
{
    public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
    {
        private readonly List<IEvent> _events = new();

        protected AggregateRoot(TId id) : base(id) { }

        protected AggregateRoot() { }

        /// <summary>
        /// Domain events raised by this aggregate.
        /// Read-only access for infrastructure to publish events.
        /// </summary>
        public IReadOnlyCollection<IEvent> Events => _events.AsReadOnly();

        /// <summary>
        /// Raise a domain event to be published after transaction commit.
        /// </summary>
        protected void RaiseEvent(IEvent e)
        {
            Guard.Against.Null(e, nameof(e));
            _events.Add(e);
        }

        /// <summary>
        /// Clear all domain events (called by infrastructure after publishing).
        /// </summary>
        public void ClearEvents()
        {
            _events.Clear();
        }
        public void RemoveEvent(IEvent e)
        {
            Guard.Against.Null(e, nameof(e));
            _events.Remove(e);
        }

        /// <summary>
        /// Check if aggregate has pending domain events.
        /// </summary>
        public bool HasEvents() => _events.Any();
    }
}
