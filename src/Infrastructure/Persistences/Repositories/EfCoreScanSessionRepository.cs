using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.AccessControl;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistences.Repositories
{
    internal sealed class EfCoreScanSessionRepository : IScanSessionRepository
    {
        private readonly AppDbContext _context;

        public EfCoreScanSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScanSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ScanSessions
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ScanSession>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.ScanSessions
                .Include(s => s.Events)
                .Where(s => s.EventId == eventId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ScanSession session, CancellationToken cancellationToken = default)
        {
            await _context.ScanSessions.AddAsync(session, cancellationToken);
        }

        public Task UpdateAsync(ScanSession session, CancellationToken cancellationToken = default)
        {
            _context.ScanSessions.Update(session);
            return Task.CompletedTask;
        }
    }
}
