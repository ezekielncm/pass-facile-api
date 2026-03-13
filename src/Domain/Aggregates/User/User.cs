using System;
using System.Collections.Generic;
using System.Text;
using Domain.ValueObjects.Identities;
using Domain.ValueObjects;
using Domain.Common;
using Domain.DomainEvents.User;

namespace Domain.Aggregates.User
{
    public sealed class User: AggregateRoot<UserId>
    {
        public UserId UserId { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public UserProfile Profile { get; private set; }
        private readonly List<Role> _roles = new();
        //public bool isActive { get; private set; } = false;
        private User() { }
        public void AddRole(Role role)
        {
            if (!_roles.Contains(role))
            {
                _roles.Add(role);
                RaiseEvent(new UserRoleAssigned(UserId, role));
            }
        }
        public void OtpVerified()
        {
            RaiseEvent(new OtpVerified(UserId));
        }

        public void Register(PhoneNumber phoneNumber, UserProfile profile)
        {
            UserId = UserId.NewId();
            PhoneNumber = phoneNumber;
            Profile = profile;
            RaiseEvent(new UserRegistered(UserId, phoneNumber, profile));
        }
    }
}
