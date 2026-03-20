using NotificationSystem.Core.Enums;

namespace NotificationSystem.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public NotificationType Type { get; init; }
        public string Recipient { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int RetryCount { get; private set; } = 0;

        public void MarkAsProcessing()
        {
            Status = NotificationStatus.Processing;
        }

        public void MarkAsSent()
        {
            Status = NotificationStatus.Sent;
            ProcessedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = NotificationStatus.Failed;
            ProcessedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            RetryCount++;
        }

        public void ResetToPending()
        {
            Status = NotificationStatus.Pending;
            ErrorMessage = null;
        }
    }
}
