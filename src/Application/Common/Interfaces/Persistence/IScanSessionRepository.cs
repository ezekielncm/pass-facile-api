using Domain.Aggregates.AccessControl;

namespace Application.Common.Interfaces.Persistence
{
    public interface IScanSessionRepository
    {
        Task<ScanSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ScanSession>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task AddAsync(ScanSession session, CancellationToken cancellationToken = default);
        Task UpdateAsync(ScanSession session, CancellationToken cancellationToken = default);
    }
}
