using Domain.Aggregates.User;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.UserId);

            builder.Property(x => x.UserId)
                .HasConversion(
                    id => id.Id,
                    value => UserId.FromGuid(value));

            builder.OwnsOne(x => x.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value)
                     .HasColumnName("PhoneNumber")
                     .IsRequired();
            });

            builder.OwnsOne(x => x.Profile, profile =>
            {
                profile.Property(p => p.FirstName).HasMaxLength(100);
                profile.Property(p => p.LastName).HasMaxLength(100);
            });

            builder.Property<List<Role>>("_roles")
                .HasColumnName("Roles");
        }
    }
}