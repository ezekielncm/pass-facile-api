using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record EventSlug : ValueObject
    {
        public string Value { get; }

        private EventSlug(string value)
        {
            Value = value;
        }
        public EventSlug() { }
        public static EventSlug Create(string value)
        {
            Guard.Against.NullOrEmpty(value, nameof(value));

            var normalized = value.Trim().ToLowerInvariant();

            // slug minimal: lettres, chiffres, tirets, 3-80 caractères
            if (normalized.Length is < 3 or > 80)
            {
                throw new BusinessRuleValidationException("EventSlug.Length",
                    "Le slug d'événement doit contenir entre 3 et 80 caractères.");
            }

            if (!normalized.All(c => char.IsLetterOrDigit(c) || c == '-'))
            {
                throw new BusinessRuleValidationException("EventSlug.InvalidChars",
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
