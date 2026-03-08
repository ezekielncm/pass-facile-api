using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects.Identities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events
{
    public sealed record UserRoleAssigned
    (UserId UserId,
        Role Role) : IEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid EventId { get; init; } = Guid.NewGuid();
    }
}
