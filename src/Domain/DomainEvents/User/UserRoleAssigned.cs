using Domain.Common;
using Domain.ValueObjects.Identities;

namespace Domain.DomainEvents.User
{
    public sealed record UserRoleAssigned(
        UserId UserId,
        string RoleName,
        string Context) : DomainEvent;
}

