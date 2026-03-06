using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Common
{
    
        public abstract class Entity<TId> : IEquatable<Entity<TId>>
        where TId : notnull
        {
            public TId Id { get; protected set; }

            protected Entity(TId id)
            {
                Guard.Against.Null(id, nameof(id));
                Id = id;
            }

            // Required for EF Core
            protected Entity() { }

            #region Equality

            public bool Equals(Entity<TId>? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                if (GetType() != other.GetType()) return false;

                return Id.Equals(other.Id);
            }

            public override bool Equals(object? obj)
            {
                return obj is Entity<TId> entity && Equals(entity);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
            {
                return !Equals(left, right);
            }

            #endregion
        }
    
}
