using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;

namespace NotificationSystem.Core.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification, CancellationToken ct = default);
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken ct = default);
        Task UpdateAsync(Notification notification, CancellationToken ct = default);
    }
}
