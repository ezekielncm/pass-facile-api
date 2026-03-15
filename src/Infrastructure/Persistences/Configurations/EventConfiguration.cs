using Domain.Aggregates.Event;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistences.Configurations
{
    public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => EventId.From(value))
                .HasColumnName("Id");

            builder.Property(e => e.Slug)
                .HasConversion(
                    s => s.Value,
                    value => EventSlug.Create(value))
                .HasMaxLength(80)
                .IsRequired();

            builder.HasIndex(e => e.Slug)
                .IsUnique();

            builder.OwnsOne(e => e.Venue, venue =>
            {
                venue.Property(v => v.Name).HasMaxLength(200).IsRequired().HasColumnName("Venue_Name");
                venue.Property(v => v.AddressLine1).HasMaxLength(300).IsRequired().HasColumnName("Venue_AddressLine1");
                venue.Property(v => v.AddressLine2).HasMaxLength(300).HasColumnName("Venue_AddressLine2");
                venue.Property(v => v.City).HasMaxLength(100).IsRequired().HasColumnName("Venue_City");
                venue.Property(v => v.Country).HasMaxLength(100).IsRequired().HasColumnName("Venue_Country");
            });

            builder.OwnsOne(e => e.SalesPeriod, sp =>
            {
                sp.Property(s => s.StartDate).HasColumnName("SalesPeriod_StartDate").IsRequired();
                sp.Property(s => s.EndDate).HasColumnName("SalesPeriod_EndDate").IsRequired();
            });

            builder.Property(e => e.Capacity)
                .HasConversion(
                    c => c.Total,
                    value => Capacity.From(value))
                .HasColumnName("Capacity");

            builder.Property(e => e.EventDate);
            builder.Property(e => e.IsPublished);
            builder.Property(e => e.SalesClosed);

            builder.HasMany(e => e.Categories)
                .WithOne()
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(e => e.Categories)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(e => e.PromoCodes)
                .WithOne()
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(e => e.PromoCodes)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(e => e.Events);
        }
    }

    public sealed class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
    {
        public void Configure(EntityTypeBuilder<TicketCategory> builder)
        {
            builder.ToTable("TicketCategories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.EventId)
                .HasConversion(
                    id => id.Value,
                    value => EventId.From(value))
                .IsRequired();

            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Price)
                .HasPrecision(18, 2);

            builder.Property(c => c.Quota);
            builder.Property(c => c.IsActive);
        }
    }

    public sealed class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
    {
        public void Configure(EntityTypeBuilder<PromoCode> builder)
        {
            builder.ToTable("PromoCodes");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.EventId)
                .HasConversion(
                    id => id.Value,
                    value => EventId.From(value))
                .IsRequired();

            builder.Property(p => p.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(p => new { p.EventId, p.Code })
                .IsUnique();

            builder.Property(p => p.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(p => p.ExpiresAt);
            builder.Property(p => p.IsActive);
        }
    }
}
