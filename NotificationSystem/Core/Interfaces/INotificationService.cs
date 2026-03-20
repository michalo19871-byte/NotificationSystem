using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;

namespace NotificationSystem.Core.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateAsync(NotificationType type, string recipient, string subject, string body, 
                                       CancellationToken ct = default);
        Task<Notification?> GetStatusAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default);
    }
}
