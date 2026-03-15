using Domain.Aggregates.Finance;

namespace Application.Common.Interfaces.Persistence
{
    public interface IOrganizerWalletRepository
    {
        Task<OrganizerWallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrganizerWallet?> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default);
        Task AddAsync(OrganizerWallet wallet, CancellationToken cancellationToken = default);
        Task UpdateAsync(OrganizerWallet wallet, CancellationToken cancellationToken = default);
    }
}
