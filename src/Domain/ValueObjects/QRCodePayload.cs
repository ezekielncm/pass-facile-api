using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed record QRCodePayload : ValueObject
    {
        public string TicketRef { get; }
        public string EventId { get; }
        public string CategoryId { get; }
        public string Signature { get; }

        private QRCodePayload(string ticketRef, string eventId, string categoryId, string signature)
        {
            TicketRef = ticketRef;
            EventId = eventId;
            CategoryId = categoryId;
            Signature = signature;
        }
        public QRCodePayload() { }

        public static QRCodePayload Create(string ticketRef, string eventId, string categoryId, string signature)
        {
            Guard.Against.NullOrEmpty(ticketRef, nameof(ticketRef));
            Guard.Against.NullOrEmpty(eventId, nameof(eventId));
            Guard.Against.NullOrEmpty(categoryId, nameof(categoryId));
            Guard.Against.NullOrEmpty(signature, nameof(signature));
            return new QRCodePayload(ticketRef.Trim(), eventId.Trim(), categoryId.Trim(), signature.Trim());
        }

        /// <summary>
        /// Backward-compatible factory from encoded string value.
        /// </summary>
        public static QRCodePayload From(string value)
        {
            Guard.Against.NullOrEmpty(value, nameof(value));
            return new QRCodePayload(value.Trim(), string.Empty, string.Empty, string.Empty);
        }

        public QRCodePayload Sign(string secret)
        {
            Guard.Against.NullOrEmpty(secret, nameof(secret));
            var sig = Convert.ToBase64String(
                System.Security.Cryptography.HMACSHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(secret),
                    System.Text.Encoding.UTF8.GetBytes($"{TicketRef}:{EventId}:{CategoryId}")));
            return new QRCodePayload(TicketRef, EventId, CategoryId, sig);
        }

        public bool Verify(string secret)
        {
            var expected = Sign(secret);
            return Signature == expected.Signature;
        }

        public string Encode() => $"{TicketRef}:{EventId}:{CategoryId}:{Signature}";

        public string Value => Encode();

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return TicketRef;
            yield return EventId;
            yield return CategoryId;
            yield return Signature;
        }

        public override string ToString() => Encode();
    }
}

