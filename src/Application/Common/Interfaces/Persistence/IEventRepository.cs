using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Event;
using Domain.ValueObjects.Identities;

namespace Application.Common.Interfaces.Persistence
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(EventId id, CancellationToken cancellationToken = default);

        Task AddAsync(Event @event, CancellationToken cancellationToken = default);

        Task UpdateAsync(Event @event, CancellationToken cancellationToken = default);
        Task DeleteAsync(Event @event,CancellationToken cancellationToken = default);
        Task<bool> ExistAsync(Event @event);
    }
}
