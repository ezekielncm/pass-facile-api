namespace Domain.Common
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Get all components that define equality for this value object.
        /// </summary>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public bool Equals(ValueObject? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override bool Equals(object? obj)
        {
            return obj is ValueObject valueObject && Equals(valueObject);
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Where(x => x != null)
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + obj!.GetHashCode();
                    }
                });
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !Equals(left, right);
        }
    }
}
