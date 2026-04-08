using Domain.Common;
using Domain.DomainEvents.Ticketing;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Aggregates.Ticketing
{
    public sealed class Ticket : AggregateRoot<Guid>
    {
        public Guid OrderId { get; private set; }
        public Guid EventId { get; private set; }
        public Guid CategoryId { get; private set; }
        public PhoneNumber BuyerPhone { get; private set; } = null!;
        public string BuyerName { get; private set; } = null!;
        public TicketReference Reference { get; private set; } = null!;
        public TicketStatus Status { get; private set; }
        public DateTimeOffset IssuedAt { get; private set; }

        public QRCode? QRCode { get; private set; }

        // EF
        private Ticket() { }

        private Ticket(
            Guid id,
            Guid orderId,
            Guid eventId,
            Guid categoryId,
            PhoneNumber buyerPhone,
            string buyerName,
            TicketReference reference)
            : base(id)
        {
            OrderId = orderId;
            EventId = eventId;
            CategoryId = categoryId;
            BuyerPhone = buyerPhone;
            BuyerName = buyerName;
            Reference = reference;
            Status = TicketStatus.Issued;
            IssuedAt = DateTimeOffset.UtcNow;
        }

        public static Ticket Issue(
            TicketReference reference,
            Guid orderId,
            Guid eventId,
            Guid categoryId,
            PhoneNumber buyerPhone,
            string buyerName)
        {
            Guard.Against.Null(buyerPhone, nameof(buyerPhone));
            Guard.Against.NullOrEmpty(buyerName, nameof(buyerName));

            var ticket = new Ticket(Guid.NewGuid(), orderId, eventId, categoryId, buyerPhone, buyerName, reference);
            ticket.RaiseEvent(new TicketIssued(ticket.Id, reference));
            return ticket;
        }

        public void AttachQrCode(QRCodePayload payload)
        {
            if (Status != TicketStatus.Issued)
            {
                throw new BusinessRuleValidationException("Ticket.NotIssued",
                    "Un QR code ne peut être généré que pour un ticket émis.");
            }

            QRCode = QRCode.Create(Id, payload);
            RaiseEvent(new QRCodeGenerated(Id, payload));
        }

        public void MarkDelivered()
        {
            if (Status != TicketStatus.Issued) return;
            RaiseEvent(new TicketDelivered(Id));
        }

        public void Revoke()
        {
            if (Status == TicketStatus.Revoked) return;

            Status = TicketStatus.Revoked;
            RaiseEvent(new TicketRevoked(Id));
        }

        public void MarkUsed()
        {
            if (Status == TicketStatus.Revoked)
            {
                throw new BusinessRuleValidationException("Ticket.RevokedCannotBeUsed",
                    "Un ticket révoqué ne peut plus être scanné.");
            }

            if (Status == TicketStatus.Used)
            {
                throw new BusinessRuleValidationException("Ticket.AlreadyUsed",
                    "Un ticket ne peut être scanné qu'une seule fois.");
            }

            Status = TicketStatus.Used;
        }
    }

    public sealed class QRCode : Entity<Guid>
    {
        public Guid TicketId { get; private set; }
        public QRCodePayload Payload { get; private set; } = null!;
        public DateTimeOffset GeneratedAt { get; private set; }
        public DateTimeOffset? ExpiresAt { get; private set; }

        // EF
        private QRCode() { }

        private QRCode(Guid id, Guid ticketId, QRCodePayload payload, DateTimeOffset? expiresAt = null)
            : base(id)
        {
            TicketId = ticketId;
            Payload = payload;
            GeneratedAt = DateTimeOffset.UtcNow;
            ExpiresAt = expiresAt;
        }

        public static QRCode Create(Guid ticketId, QRCodePayload payload, DateTimeOffset? expiresAt = null)
        {
            return new QRCode(Guid.NewGuid(), ticketId, payload, expiresAt);
        }
    }
}

