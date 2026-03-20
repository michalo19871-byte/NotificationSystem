using System.ComponentModel.DataAnnotations;
using NotificationSystem.Core.Enums;

namespace NotificationSystem.API.DTOs
{
    public sealed record CreateNotificationRequest
    {
        [Required]
        public NotificationType Type { get; init; }

        [Required, EmailAddress]
        public string Recipient { get; init; } = string.Empty;

        [Required, MaxLength(200)]
        public string Subject { get; init; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Body { get; init; } = string.Empty;
    }
}
