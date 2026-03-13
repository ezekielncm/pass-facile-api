using Domain.Common;
using Domain.ValueObjects.Identities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DomainEvents.User
{
    public sealed record OtpVerified(
        UserId UserId
        //string Otp
        ) : IEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid EventId { get; init; } = Guid.NewGuid();
    }
}
