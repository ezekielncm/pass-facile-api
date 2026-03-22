using Domain.Common;
using Domain.Enums;

namespace Domain.ValueObjects
{
    public sealed record ScanResult : ValueObject
    {
        public ScanStatus Status { get; }
        public string? TicketRef { get; }
        public string? Category { get; }
        public string Message { get; }

        private ScanResult(ScanStatus status, string? ticketRef, string? category, string message)
        {
            Status = status;
            TicketRef = ticketRef;
            Category = category;
            Message = message;
        }
        public ScanResult() { }

        public static ScanResult Create(ScanStatus status, string message, string? ticketRef = null, string? category = null)
        {
            Guard.Against.NullOrEmpty(message, nameof(message));
            return new ScanResult(status, ticketRef, category, message);
        }

        /// <summary>
        /// Backward-compatible factory from string value.
        /// </summary>
        public static ScanResult From(string value)
        {
            Guard.Against.NullOrEmpty(value, nameof(value));
            var upper = value.Trim().ToUpperInvariant();
            var status = upper switch
            {
                "VALID" => ScanStatus.Valid,
                "INVALID" => ScanStatus.Invalid,
                "ALREADYUSED" or "DUPLICATE" => ScanStatus.AlreadyUsed,
                "EXPIRED" => ScanStatus.Expired,
                _ => ScanStatus.Invalid
            };
            return new ScanResult(status, null, null, upper);
        }

        public string Value => Status.ToString().ToUpperInvariant();

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Status;
            yield return TicketRef;
            yield return Category;
            yield return Message;
        }
    }
}

