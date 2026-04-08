using Domain.Aggregates.Finance;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistences.Configurations
{
    public sealed class OrganizerWalletConfiguration : IEntityTypeConfiguration<OrganizerWallet>
    {
        public void Configure(EntityTypeBuilder<OrganizerWallet> builder)
        {
            builder.ToTable("OrganizerWallets");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.OrganizerId);

            builder.HasIndex(w => w.OrganizerId)
                .IsUnique();

            builder.Property(w => w.Currency)
                .HasMaxLength(5)
                .IsRequired();

            builder.OwnsOne(w => w.Balance, balance =>
            {
                balance.OwnsOne(b => b.Available, money =>
                {
                    money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Balance_Available_Amount").IsRequired();
                    money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Balance_Available_Currency").IsRequired();
                });

                balance.OwnsOne(b => b.Pending, money =>
                {
                    money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Balance_Pending_Amount").IsRequired();
                    money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Balance_Pending_Currency").IsRequired();
                });
            });

            builder.HasMany(w => w.Withdrawals)
                .WithOne()
                .HasForeignKey(wr => wr.OrganizerWalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(w => w.Withdrawals)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(w => w.Fees)
                .WithOne()
                .HasForeignKey(f => f.OrganizerWalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(w => w.Fees)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(w => w.Events);
        }
    }

    public sealed class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
    {
        public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
        {
            builder.ToTable("WithdrawalRequests");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.OrganizerWalletId);
            builder.Property(w => w.AccountId);

            builder.OwnsOne(w => w.Amount, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("Amount_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("Amount_Currency").IsRequired();
            });

            builder.Property(w => w.Status)
                .HasConversion<int>();

            builder.Property(w => w.RequestedAt);
            builder.Property(w => w.ProcessedAt);
        }
    }

    public sealed class FeeTransactionConfiguration : IEntityTypeConfiguration<FeeTransaction>
    {
        public void Configure(EntityTypeBuilder<FeeTransaction> builder)
        {
            builder.ToTable("FeeTransactions");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.OrganizerWalletId);
            builder.Property(f => f.OrderId);
            builder.Property(f => f.EventId);

            builder.OwnsOne(f => f.GrossAmount, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("GrossAmount_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("GrossAmount_Currency").IsRequired();
            });

            builder.OwnsOne(f => f.PlatformFee, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("PlatformFee_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("PlatformFee_Currency").IsRequired();
            });

            builder.OwnsOne(f => f.NetAmount, money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).HasColumnName("NetAmount_Amount").IsRequired();
                money.Property(m => m.Currency).HasMaxLength(5).HasColumnName("NetAmount_Currency").IsRequired();
            });

            builder.Property(f => f.CreatedAt);
        }
    }
}
