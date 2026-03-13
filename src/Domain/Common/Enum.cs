namespace Domain.Common
{
    public abstract record Enum : IEquatable<Enum>
    {
        public int Id { get; protected set; }
        public string Name { get; protected set; } = string.Empty;

        protected Enum() { }

        protected Enum(int id, string name)
        {
            Guard.Against.Null(name, nameof(name));
            Guard.Against.NegativeOrZero(id, nameof(id));

            Id = id;
            Name = name;
        }

        public virtual bool Equals(Enum? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return Id == other.Id;
        }

        //public override bool Equals(object? obj)
        //{
        //    return obj is Enum @enum && Equals(@enum);
        //}

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        //public static bool operator ==(Enum? left, Enum? right)
        //{
        //    return Equals(left, right);
        //}

        //public static bool operator !=(Enum? left, Enum? right)
        //{
        //    return !Equals(left, right);
        //}
    }
}
