using Domain.Aggregates.AccessControl;
using Domain.Enums;
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

            builder.Property(s => s.StartedAt);
            builder.Property(s => s.IsOffline);

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

            builder.Property(e => e.QrPayload)
                .HasMaxLength(2000)
                .IsRequired();

            builder.OwnsOne(e => e.Result, r =>
            {
                r.Property(x => x.Status).HasConversion<int>().HasColumnName("Result_Status").IsRequired();
                r.Property(x => x.TicketRef).HasMaxLength(100).HasColumnName("Result_TicketRef");
                r.Property(x => x.Category).HasMaxLength(100).HasColumnName("Result_Category");
                r.Property(x => x.Message).HasMaxLength(500).HasColumnName("Result_Message").IsRequired();
            });

            builder.Property(e => e.ScannedAt);
            builder.Property(e => e.SyncedAt);
        }
    }
}
