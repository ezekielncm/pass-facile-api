using Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces.Messaging
{
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes a single domain event asynchronously.
        /// </summary>
        /// <typeparam name="T">Type of the domain event.</typeparam>
        /// <param name="domainEvent">The domain event to publish.</param>
        Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
            where T : IDomainEvent;

        /// <summary>
        /// Publishes multiple domain events asynchronously.
        /// </summary>
        /// <param name="domainEvents">The collection of domain events to publish.</param>
        Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
