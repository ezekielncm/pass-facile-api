using Application.Common.Interfaces.Messaging;
using Domain.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger<EventPublisher> _logger;

        public EventPublisher(ILogger<EventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
            where T : IEvent
        {
            return PublishAsync(new[] { (IEvent)domainEvent }, cancellationToken);
        }

        public Task PublishAsync(IEnumerable<IEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            foreach (var ev in domainEvents)
            {
                _logger.LogInformation("Publishing domain event {EventType} (Id={EventId}) at {OccurredAt}", ev.GetType().Name, ev.EventId, ev.OccurredAt);
            }

            return Task.CompletedTask;
        }
    }
}
