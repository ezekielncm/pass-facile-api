using Domain.Aggregates.AccessControl;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistences.Configurations
{
    public sealed class ScanSessionConfiguration : IEntityTypeConfiguration<ScanSession>
    {
        public void Configure(EntityTypeBuilder<ScanSession> builder)
        {
            builder.ToTable("ScanSessions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.EventId);
            builder.Property(s => s.AgentId);

            builder.Property(s => s.DeviceId)
                .HasConversion(
                    d => d.Value,
                    value => DeviceId.From(value))
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(s => s.IsOffline);
            builder.Property(s => s.CreatedAt);
            builder.Property(s => s.SyncedAt);

            builder.HasMany(s => s.Events)
                .WithOne()
                .HasForeignKey(e => e.ScanSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(s => s.Events)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    public sealed class ScanEventConfiguration : IEntityTypeConfiguration<ScanEvent>
    {
        public void Configure(EntityTypeBuilder<ScanEvent> builder)
        {
            builder.ToTable("ScanEvents");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.ScanSessionId);
            builder.Property(e => e.TicketId);

            builder.Property(e => e.Result)
                .HasConversion(
                    r => r.Value,
                    value => ScanResult.From(value))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.ScannedAt);
        }
    }
}
