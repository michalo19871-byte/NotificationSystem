using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;
using NotificationSystem.Core.Interfaces;

namespace NotificationSystem.Infrastructure.Senders
{
    /// <summary>
    /// Simulates sending an email. Replace internals with SMTP / SendGrid / etc.
    /// </summary>
    public sealed class EmailSender : INotificationSender
    {
        private readonly ILogger<EmailSender> _logger;

        public NotificationType SupportedType => NotificationType.Email;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(Notification notification, CancellationToken ct = default)
        {
            // Simulate I/O latency (e.g. SMTP roundtrip)
            await Task.Delay(TimeSpan.FromMilliseconds(200), ct);

            _logger.LogInformation($"Email to: {notification.Recipient} | Subject: {notification.Subject} " +
                                   $"| Body: {notification.Body}");
        }
    }
}
