using Domain.Common;
using Domain.DomainEvents.User;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Domain.Aggregates.User
{
    /// <summary>
    /// Agrégat racine User (identité métier).
    /// </summary>
    public sealed class User : AggregateRoot<UserId>
    {
        private readonly List<UserRole> _roles = [];

        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public UserProfile? Profile { get; private set; }
        public bool PhoneVerified { get; private set; }

        public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

        // EF
        private User() { }

        private User(UserId id, PhoneNumber phoneNumber, UserProfile? profile)
            : base(id)
        {
            Id = id;
            PhoneNumber = phoneNumber;
            Profile = profile;

            RaiseEvent(new UserRegistered(Id, phoneNumber, profile));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur identifié uniquement par son PhoneNumber.
        /// </summary>
        public static User Register(PhoneNumber phoneNumber, UserProfile? profile = null)
        {
            Guard.Against.Null(phoneNumber, nameof(phoneNumber));
            var id = UserId.NewId();
            return new User(id, phoneNumber, profile);
        }

        /// <summary>
        /// Marque l'OTP comme vérifié et donc le numéro de téléphone comme confirmé.
        /// </summary>
        public void MarkOtpVerified()
        {
            if (PhoneVerified)
            {
                return;
            }

            PhoneVerified = true;
            RaiseEvent(new OtpVerified(Id));
        }

        /// <summary>
        /// Assigne un rôle métier en respectant l'invariant :
        /// un seul rôle actif par contexte (Admin, Agent, Organisateur).
        /// </summary>
        public void SetContextRole(string roleName, string context)
        {
            Guard.Against.NullOrEmpty(roleName, nameof(roleName));
            Guard.Against.NullOrEmpty(context, nameof(context));

            // désactiver les rôles existants pour ce contexte
            foreach (var r in _roles.Where(r => r.Context.Equals(context, StringComparison.OrdinalIgnoreCase)))
            {
                r.Deactivate();
            }

            var newRole = new UserRole(Guid.NewGuid(), roleName, context, isActive: true);
            _roles.Add(newRole);

            RaiseEvent(new UserRoleAssigned(Id, roleName, context));
        }

        public void UpdateProfile(UserProfile profile)
        {
            Guard.Against.Null(profile, nameof(profile));
            Profile = profile;
        }
    }
}

