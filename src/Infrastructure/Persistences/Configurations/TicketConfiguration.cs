using Domain.Aggregates.Ticketing;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistences.Configurations
{
    public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Reference)
                .HasConversion(
                    r => r.Value,
                    value => TicketReference.From(value))
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(t => t.Reference)
                .IsUnique();

            builder.Property(t => t.OrderId);
            builder.Property(t => t.EventId);
            builder.Property(t => t.IsIssued);
            builder.Property(t => t.IsRevoked);
            builder.Property(t => t.IsUsed);

            builder.HasOne(t => t.QRCode)
                .WithOne()
                .HasForeignKey<QRCode>(q => q.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(t => t.Events);
        }
    }

    public sealed class QRCodeConfiguration : IEntityTypeConfiguration<QRCode>
    {
        public void Configure(EntityTypeBuilder<QRCode> builder)
        {
            builder.ToTable("QRCodes");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.TicketId)
                .IsRequired();

            builder.Property(q => q.Payload)
                .HasConversion(
                    p => p.Value,
                    value => QRCodePayload.From(value))
                .HasMaxLength(2000)
                .IsRequired();
        }
    }
}
