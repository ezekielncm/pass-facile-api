using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.Event;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistences.Repositories
{
    internal sealed class EfCoreEventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EfCoreEventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(EventId id, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.PromoCodes)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
        public async Task<Event?> GetBySlugAsync(EventSlug slug, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.PromoCodes)
                .FirstOrDefaultAsync(e => e.Slug == slug, cancellationToken);
        }

        public async Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.PromoCodes)
                .ToListAsync(cancellationToken);
        }
        public async Task<List<Event>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.PromoCodes)
                .Where(e => e.Categories.Any(c => c.Id == categoryId))
                .ToListAsync(cancellationToken);
        }
        public async Task<List<Event>> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.PromoCodes)
                .Where(e => e.OrganizerId == organizerId)
                .ToListAsync(cancellationToken);
        }
        public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
        {
            await _context.Events.AddAsync(@event, cancellationToken);
        }

        public Task UpdateAsync(Event @event, CancellationToken cancellationToken = default)
        {
            _context.Events.Update(@event);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Event @event, CancellationToken cancellationToken = default)
        {
            _context.Events.Remove(@event);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistAsync(Event @event)
        {
            return await _context.Events.AnyAsync(e => e.Id == @event.Id);
        }
    }
}
