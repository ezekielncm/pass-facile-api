using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class EventSlug : ValueObject
    {
        public string Value { get; }

        private EventSlug(string value)
        {
            Value = value;
        }

        public static EventSlug Create(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));

            var normalized = value.Trim().ToLowerInvariant();

            // slug minimal: lettres, chiffres, tirets, 3-80 caractères
            if (normalized.Length is < 3 or > 80)
            {
                throw new DomainException("EventSlug.Length",
                    "Le slug d'événement doit contenir entre 3 et 80 caractères.");
            }

            if (!normalized.All(c => char.IsLetterOrDigit(c) || c == '-'))
            {
                throw new DomainException("EventSlug.InvalidChars",
                    "Le slug d'événement ne peut contenir que des lettres, chiffres ou tirets.");
            }

            return new EventSlug(normalized);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
