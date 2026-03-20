using NotificationSystem.Core.Entities;

namespace NotificationSystem.API.DTOs
{
    public static class NotificationMapper
    {
        public sealed record NotificationResponse(
            Guid Id,
            string Type,
            string Recipient,
            string Subject,
            string Body,
            string Status,
            DateTime CreatedAt,
            DateTime? ProcessedAt,
            string? ErrorMessage,
            int RetryCount
        );

        public static NotificationResponse ToResponse(Notification notification) => new(
            notification.Id,
            notification.Type.ToString(),
            notification.Recipient,
            notification.Subject,
            notification.Body,
            notification.Status.ToString(),
            notification.CreatedAt,
            notification.ProcessedAt,
            notification.ErrorMessage,
            notification.RetryCount
        );
    }
}
