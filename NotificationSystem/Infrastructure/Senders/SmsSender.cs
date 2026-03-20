using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;
using NotificationSystem.Core.Interfaces;

namespace NotificationSystem.Infrastructure.Senders
{
    /// <summary>
    /// Simulates sending an SMS. Replace internals with Twilio / AWS SNS / etc.
    /// </summary>
    public sealed class SmsSender : INotificationSender
    {
        private readonly ILogger<SmsSender> _logger;

        public NotificationType SupportedType => NotificationType.Sms;

        public SmsSender(ILogger<SmsSender> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(Notification notification, CancellationToken ct = default)
        {
            // Simulate I/O latency (e.g. SMS gateway call)
            await Task.Delay(TimeSpan.FromMilliseconds(150), ct);

            _logger.LogInformation($"Sms to: {notification.Recipient} | Message: {notification.Body}");
        }
    }
}
