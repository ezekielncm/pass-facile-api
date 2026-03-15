namespace Domain.Common
{
    public abstract record ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Get all components that define equality for this value object.
        /// </summary>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public virtual bool Equals(ValueObject? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        //public override bool Equals(object? obj)
        //{
        //    return obj is ValueObject valueObject && Equals(valueObject);
        //}

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Where(component => component != null)
                .Aggregate(1, (current, component) =>
                {
                    unchecked
                    {
                        return current * 23 + component!.GetHashCode();
                    }
                });
        }

        //public static bool operator ==(ValueObject? left, ValueObject? right)
        //{
        //    return Equals(left, right);
        //}

        //public static bool operator !=(ValueObject? left, ValueObject? right)
        //{
        //    return !Equals(left, right);
        //}
    }
}
