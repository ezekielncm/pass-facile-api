using Domain.Common;
using Domain.DomainEvents.Ticketing;
using Domain.ValueObjects;

namespace Domain.Aggregates.Ticketing
{
    public sealed class Ticket : AggregateRoot<Guid>
    {
        public TicketReference Reference { get; private set; } = null!;
        public Guid OrderId { get; private set; }
        public Guid EventId { get; private set; }
        public bool IsIssued { get; private set; }
        public bool IsRevoked { get; private set; }
        public bool IsUsed { get; private set; }

        public QRCode? QRCode { get; private set; }

        // EF
        private Ticket() { }

        private Ticket(
            Guid id,
            TicketReference reference,
            Guid orderId,
            Guid eventId)
            : base(id)
        {
            Reference = reference;
            OrderId = orderId;
            EventId = eventId;
        }

        public static Ticket Issue(
            TicketReference reference,
            Guid orderId,
            Guid eventId)
        {
            var ticket = new Ticket(Guid.NewGuid(), reference, orderId, eventId)
            {
                IsIssued = true
            };

            ticket.RaiseEvent(new TicketIssued(ticket.Id, reference));
            return ticket;
        }

        public void AttachQrCode(QRCodePayload payload)
        {
            if (!IsIssued)
            {
                throw new DomainException("Ticket.NotIssued",
                    "Un QR code ne peut être généré que pour un ticket émis.");
            }

            QRCode = QRCode.Create(Id, payload);
            RaiseEvent(new QRCodeGenerated(Id, payload));
        }

        public void MarkDelivered()
        {
            if (!IsIssued) return;
            RaiseEvent(new TicketDelivered(Id));
        }

        public void Revoke()
        {
            if (IsRevoked) return;

            IsRevoked = true;
            RaiseEvent(new TicketRevoked(Id));
        }

        public void MarkUsed()
        {
            if (IsRevoked)
            {
                throw new DomainException("Ticket.RevokedCannotBeUsed",
                    "Un ticket révoqué ne peut plus être scanné.");
            }

            if (IsUsed)
            {
                throw new DomainException("Ticket.AlreadyUsed",
                    "Un ticket ne peut être scanné qu'une seule fois.");
            }

            IsUsed = true;
        }
    }

    public sealed class QRCode : Entity<Guid>
    {
        public Guid TicketId { get; private set; }
        public QRCodePayload Payload { get; private set; } = null!;

        // EF
        private QRCode() { }

        private QRCode(Guid id, Guid ticketId, QRCodePayload payload)
            : base(id)
        {
            TicketId = ticketId;
            Payload = payload;
        }

        public static QRCode Create(Guid ticketId, QRCodePayload payload)
        {
            return new QRCode(Guid.NewGuid(), ticketId, payload);
        }
    }
}

