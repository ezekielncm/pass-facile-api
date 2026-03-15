using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.Ticketing;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistences.Repositories
{
    internal sealed class EfCoreTicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;

        public EfCoreTicketRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Ticketes
                .Include(t => t.QRCode)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<Ticket?> GetByReferenceAsync(TicketReference reference, CancellationToken cancellationToken = default)
        {
            return await _context.Ticketes
                .Include(t => t.QRCode)
                .FirstOrDefaultAsync(t => t.Reference == reference, cancellationToken);
        }

        public async Task<IReadOnlyList<Ticket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Ticketes
                .Include(t => t.QRCode)
                .Where(t => t.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.Ticketes
                .Include(t => t.QRCode)
                .Where(t => t.EventId == eventId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            await _context.Ticketes.AddAsync(ticket, cancellationToken);
        }

        public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            _context.Ticketes.Update(ticket);
            return Task.CompletedTask;
        }
    }
}
