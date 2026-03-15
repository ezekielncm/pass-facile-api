using Domain.Aggregates.User;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistences.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasConversion(
                    id => id.Value,
                    value => UserId.FromGuid(value))
                .HasColumnName("Id");

            builder.Property(u => u.PhoneNumber)
                .HasConversion(
                    p => p.Value,
                    value => new PhoneNumber(value))
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique();

            builder.OwnsOne(u => u.Profile, profile =>
            {
                profile.Property(p => p.DisplayName).HasMaxLength(100).HasColumnName("Profile_DisplayName");
                profile.Property(p => p.Bio).HasMaxLength(500).HasColumnName("Profile_Bio");
                profile.Property(p => p.LogoUrl).HasMaxLength(500).HasColumnName("Profile_LogoUrl");
                profile.Property(p => p.BannerUrl).HasMaxLength(500).HasColumnName("Profile_BannerUrl");
                profile.Property(p => p.Slug).HasMaxLength(100).HasColumnName("Profile_Slug");
            });

            builder.Property(u => u.PhoneVerified);

            builder.HasMany(u => u.Roles)
                .WithOne()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(u => u.Roles)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(u => u.Events);
        }
    }

    public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles_Domain");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.Context)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.IsActive);
        }
    }

    public sealed class OTPCodeConfiguration : IEntityTypeConfiguration<OTPCode>
    {
        public void Configure(EntityTypeBuilder<OTPCode> builder)
        {
            builder.ToTable("OTPCodes");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.PhoneNumber)
                .HasConversion(
                    p => p.Value,
                    value => new PhoneNumber(value))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.Code)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(o => o.ExpiresAt);
            builder.Property(o => o.CreatedAt);
            builder.Property(o => o.Used);

            builder.HasIndex(o => o.PhoneNumber);
        }
    }
}
