using Domain.Aggregates.User;
using Domain.ValueObjects.Identities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(UserId id, CancellationToken ct);
        Task AddAsync(User user, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
