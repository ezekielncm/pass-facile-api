using System;
using System.Collections.Generic;
using System.Text;
using Domain.ValueObjects.Identities;
using Domain.ValueObjects;
using Domain.Common;

namespace Domain.DomainEvents.User
{
    public record UserRegistered(
        UserId UserId,
        PhoneNumber PhoneNumber,
        UserProfile Profile) : IEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid EventId { get; init; } = Guid.NewGuid();
    }
}
