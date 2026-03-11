using Application.Common.Interfaces.Persistence;
using Domain.Aggregates.User;
using Domain.ValueObjects.Identities;
using Infrastructure.Persistences;

public sealed class EfCoreUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EfCoreUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct)
    {
        return await _context.Users.FindAsync([id], ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}