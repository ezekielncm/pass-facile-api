using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.User
{
    public sealed record OtpVerified(UserId UserId) : DomainEvent;
}

