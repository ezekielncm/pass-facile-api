using Domain.Common;

namespace Domain.Aggregates.User
{
    /// <summary>
    /// Rôle métier assigné à un utilisateur pour un contexte donné.
    /// </summary>
    public sealed class UserRole : Entity<Guid>
    {
        public string Name { get; private set; } = null!;
        public string Context { get; private set; } = null!;
        public bool IsActive { get; private set; }

        // EF
        private UserRole() { }

        internal UserRole(Guid id, string name, string context, bool isActive)
            : base(id)
        {
            Guard.Against.NullOrEmpty(name, nameof(name));
            Guard.Against.NullOrEmpty(context, nameof(context));

            Name = name.Trim();
            Context = context.Trim();
            IsActive = isActive;
        }

        internal void Deactivate() => IsActive = false;
    }
}

