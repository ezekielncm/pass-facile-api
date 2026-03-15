using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.User;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace Application.Common.Interfaces.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

        Task<User?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
