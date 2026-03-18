using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record MessageTemplate : ValueObject
    {
        public string Code { get; }

        private MessageTemplate(string code)
        {
            Code = code;
        }
        public MessageTemplate() { }

        public static MessageTemplate From(string code)
        {
            Guard.Against.NullOrEmpty(code, nameof(code));
            return new MessageTemplate(code.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Code;
        }
    }
}

