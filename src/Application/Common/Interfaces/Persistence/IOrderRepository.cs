using Domain.Aggregates.Sales;
using Domain.ValueObjects.Identities;

namespace Application.Common.Interfaces.Persistence
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    }
}
