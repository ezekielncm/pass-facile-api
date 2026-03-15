using Domain.Aggregates.Ticketing;
using Domain.ValueObjects;

namespace Application.Common.Interfaces.Persistence
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Ticket?> GetByReferenceAsync(TicketReference reference, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Ticket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
        Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    }
}
