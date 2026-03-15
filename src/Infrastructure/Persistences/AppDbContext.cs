
using Domain.Aggregates.AccessControl;
using Domain.Aggregates.Event;
using Domain.Aggregates.Finance;
using Domain.Aggregates.Notifications;
using Domain.Aggregates.Sales;
using Domain.Aggregates.Ticketing;
using Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistences
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Ticket> Ticketes => Set<Ticket>();
        public DbSet<ScanSession> ScanSessions => Set<ScanSession>();
        public DbSet<OrganizerWallet> Organizers => Set<OrganizerWallet>();
        public DbSet<NotificationRequest> NotificationRequests => Set<NotificationRequest>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Ignore Domain Events here - they are published in UnitOfWork
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
