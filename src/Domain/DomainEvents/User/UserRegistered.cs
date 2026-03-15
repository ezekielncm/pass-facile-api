using Domain.Common;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.User
{
    public sealed record UserRegistered(
        UserId UserId,
        PhoneNumber PhoneNumber,
        UserProfile? Profile) : DomainEvent;
}

