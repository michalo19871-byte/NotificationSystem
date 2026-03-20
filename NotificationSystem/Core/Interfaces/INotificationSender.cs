using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;

namespace NotificationSystem.Core.Interfaces
{
    public interface INotificationSender
    {
        NotificationType SupportedType { get; }
        Task SendAsync(Notification notification, CancellationToken ct = default);
    }
}
