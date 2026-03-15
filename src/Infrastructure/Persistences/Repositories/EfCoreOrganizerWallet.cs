using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.Finance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistences.Repositories
{
    internal sealed class EfCoreOrganizerWalletRepository : IOrganizerWalletRepository
    {
        private readonly AppDbContext _context;

        public EfCoreOrganizerWalletRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizerWallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Organizers
                .Include(w => w.Withdrawals)
                .Include(w => w.Fees)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<OrganizerWallet?> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default)
        {
            return await _context.Organizers
                .Include(w => w.Withdrawals)
                .Include(w => w.Fees)
                .FirstOrDefaultAsync(w => w.OrganizerId == organizerId, cancellationToken);
        }

        public async Task AddAsync(OrganizerWallet wallet, CancellationToken cancellationToken = default)
        {
            await _context.Organizers.AddAsync(wallet, cancellationToken);
        }

        public Task UpdateAsync(OrganizerWallet wallet, CancellationToken cancellationToken = default)
        {
            _context.Organizers.Update(wallet);
            return Task.CompletedTask;
        }
    }
}
