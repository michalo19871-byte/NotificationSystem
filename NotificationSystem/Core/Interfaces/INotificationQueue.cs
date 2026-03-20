using NotificationSystem.Core.Entities;

namespace NotificationSystem.Core.Interfaces
{
    public interface INotificationQueue
    {
        ValueTask EnqueueAsync(Notification notification, CancellationToken ct = default);
        ValueTask<Notification?> DequeueAsync(CancellationToken ct = default);
        int Count { get; }
    }
}
