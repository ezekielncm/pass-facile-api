using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.User;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistences.Repositories
{
    internal sealed class EfCoreUserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public EfCoreUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }
    }
}
