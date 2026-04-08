using Domain.Aggregates.Sales;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistences.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasConversion(
                    id => id.Value,
                    value => OrderId.From(value))
                .HasColumnName("Id");

            builder.Property(o => o.BuyerPhone)
                .HasConversion(
                    p => p.Value,
                    value => new PhoneNumber(value))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.CategoryId);
            builder.Property(o => o.EventId);
            builder.Property(o => o.PromoCodeId);
            builder.Property(o => o.Quantity);

            builder.OwnsOne(o => o.Subtotal, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Subtotal_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Subtotal_Currency").IsRequired();
            });

            builder.OwnsOne(o => o.Fees, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Fees_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Fees_Currency").IsRequired();
            });

            builder.OwnsOne(o => o.Total, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Total_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Total_Currency").IsRequired();
            });

            builder.Property(o => o.Status)
                .HasConversion<int>();

            builder.Property(o => o.ReservedUntil);
            builder.Property(o => o.CreatedAt);

            builder.HasOne(o => o.Payment)
                .WithOne()
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(o => o.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(o => o.Refunds)
                .WithOne()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(o => o.Refunds)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(o => o.Events);
        }
    }

    public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.OrderId)
                .HasConversion(
                    id => id.Value,
                    value => OrderId.From(value))
                .IsRequired();

            builder.Property(i => i.TicketRef)
                .HasConversion(
                    r => r.Value,
                    value => TicketReference.From(value))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.Quantity);

            builder.OwnsOne(i => i.UnitPrice, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("UnitPrice_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("UnitPrice_Currency").IsRequired();
            });

            builder.Ignore(i => i.LineTotal);
        }
    }

    public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.OrderId)
                .HasConversion(
                    id => id.Value,
                    value => OrderId.From(value))
                .IsRequired();

            builder.Property(p => p.Provider)
                .HasConversion<int>();

            builder.Property(p => p.PhoneNumber)
                .HasConversion(
                    ph => ph.Value,
                    value => new PhoneNumber(value))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.TransactionId)
                .HasConversion(
                    t => t.Value,
                    value => TransactionId.From(value))
                .HasMaxLength(200)
                .IsRequired();

            builder.OwnsOne(p => p.Amount, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Amount_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Amount_Currency").IsRequired();
            });

            builder.Property(p => p.Status)
                .HasConversion<int>();

            builder.Property(p => p.InitiatedAt);
            builder.Property(p => p.ConfirmedAt);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);
        }
    }

    public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.ToTable("Refunds");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.OrderId)
                .HasConversion(
                    id => id.Value,
                    value => OrderId.From(value))
                .IsRequired();

            builder.Property(r => r.PaymentId);

            builder.OwnsOne(r => r.Amount, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Amount_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Amount_Currency").IsRequired();
            });

            builder.Property(r => r.Reason)
                .HasMaxLength(500);

            builder.Property(r => r.Status)
                .HasConversion<int>();

            builder.Property(r => r.ProcessedAt);
        }
    }
}
