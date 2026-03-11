using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistences
{
    public sealed class IDbContext
        : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public IDbContext(DbContextOptions<IDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //customize Identity table names
            builder.Entity<AppUser>(t =>
            {
                t.ToTable("Users");
            });
            builder.Entity<AppRole>(t =>
            {
                t.ToTable("Roles");
            });
            builder.Entity<IdentityUserRole<Guid>>(b =>
            {
                b.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<Guid>>(b =>
            {
                b.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<Guid>>(b =>
            {
                b.ToTable("UserLogins");
            });

            builder.Entity<IdentityUserToken<Guid>>(b =>
            {
                b.ToTable("UserTokens");
            });

            builder.Entity<IdentityRoleClaim<Guid>>(b =>
            {
                b.ToTable("RoleClaims");
            });
            // Seed default roles
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var agentRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var organisateurRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var userRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            builder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "System administrator with full access",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000010"
                },
                new AppRole
                {
                    Id = agentRoleId,
                    Name = "Agent",
                    NormalizedName = "AGENT",
                    Description = "agent",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000011"
                },
                new AppRole
                {
                    Id = organisateurRoleId,
                    Name = "Organisateur",
                    NormalizedName = "ORGANISATEUR",
                    Description = "organisateur",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000012"
                },
                new AppRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "User",
                    Description = "user",
                    ConcurrencyStamp = "00000000-0000-0000-0000-000000000013"
                }
            );

            // Seed default admin user
            var adminUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<AppUser>();

            //var adminUser = new AppUser
            //{
            //    Id = adminUserId,
            //    UserName = "admin@medinfo.local",
            //    NormalizedUserName = "ADMIN@MEDINFO.LOCAL",
            //    Email = "admin@medinfo.local",
            //    NormalizedEmail = "ADMIN@MEDINFO.LOCAL",
            //    EmailConfirmed = true,
            //    SecurityStamp = "00000000-0000-0000-0000-000000000001",
            //    ConcurrencyStamp = "00000000-0000-0000-0000-000000000002",
            //    PasswordHash = "AQAAAAEAACcQAAAAE..."
            //};

            //adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");
            //adminUser.PasswordHash = "AQAAAAEAACcQAAAAE..."; // precomputed hash

            //builder.Entity<AppUser>().HasData(adminUser);
            /*
            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                }
            );*/
        }
    }
}
