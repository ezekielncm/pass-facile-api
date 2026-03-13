using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class MessageTemplate : ValueObject
    {
        public string Code { get; }

        private MessageTemplate(string code)
        {
            Code = code;
        }

        public static MessageTemplate From(string code)
        {
            Guard.Against.NullOrWhiteSpace(code, nameof(code));
            return new MessageTemplate(code.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Code;
        }
    }
}

