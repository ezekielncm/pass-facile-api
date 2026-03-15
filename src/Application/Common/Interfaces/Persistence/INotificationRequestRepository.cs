using Domain.Aggregates.Notifications;

namespace Application.Common.Interfaces.Persistence
{
    public interface INotificationRequestRepository
    {
        Task<NotificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(NotificationRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(NotificationRequest request, CancellationToken cancellationToken = default);
    }
}
