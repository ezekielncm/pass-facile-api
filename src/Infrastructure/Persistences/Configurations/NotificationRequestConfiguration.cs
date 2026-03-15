using Domain.Aggregates.Notifications;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistences.Configurations
{
    public sealed class NotificationRequestConfiguration : IEntityTypeConfiguration<NotificationRequest>
    {
        public void Configure(EntityTypeBuilder<NotificationRequest> builder)
        {
            builder.ToTable("NotificationRequests");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Channel)
                .HasConversion(
                    c => c.Value,
                    value => Channel.From(value))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(n => n.Template)
                .HasConversion(
                    t => t.Code,
                    value => MessageTemplate.From(value))
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(n => n.Recipient)
                .HasConversion(
                    r => r.Value,
                    value => RecipientContact.From(value))
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(n => n.IsOptOut);
            builder.Property(n => n.IsQueued);

            builder.HasMany(n => n.Attempts)
                .WithOne()
                .HasForeignKey(a => a.NotificationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(n => n.Attempts)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(n => n.Events);
        }
    }

    public sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
    {
        public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
        {
            builder.ToTable("DeliveryAttempts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.NotificationRequestId);
            builder.Property(a => a.Success);
            builder.Property(a => a.AttemptedAt);
        }
    }
}
