using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.Sales;
using Domain.ValueObjects.Identities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistences.Repositories
{
    internal sealed class EfCoreOrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public EfCoreOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .Include(o => o.Refunds)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }
    }
}
