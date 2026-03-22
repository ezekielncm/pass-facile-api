using Domain.Aggregates.Notifications;
using Domain.Enums;
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

            builder.Property(n => n.RecipientPhone)
                .HasConversion(
                    p => p.Value,
                    value => new PhoneNumber(value))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(n => n.Channel)
                .HasConversion(
                    c => c.Value,
                    value => Channel.From(value))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(n => n.TemplateId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(n => n.Status)
                .HasConversion<int>();

            builder.Property(n => n.ScheduledAt);
            builder.Property(n => n.CreatedAt);

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
            builder.Property(a => a.AttemptNumber);
            builder.Property(a => a.SentAt);
            builder.Property(a => a.Success);
            builder.Property(a => a.ErrorMessage)
                .HasMaxLength(1000);
        }
    }
}
