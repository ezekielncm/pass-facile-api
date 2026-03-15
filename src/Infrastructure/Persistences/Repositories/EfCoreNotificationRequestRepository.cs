using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistences.Repositories
{
    internal sealed class EfCoreNotificationRequestRepository : INotificationRequestRepository
    {
        private readonly AppDbContext _context;

        public EfCoreNotificationRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.NotificationRequests
                .Include(n => n.Attempts)
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task AddAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            await _context.NotificationRequests.AddAsync(request, cancellationToken);
        }

        public Task UpdateAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            _context.NotificationRequests.Update(request);
            return Task.CompletedTask;
        }
    }
}
