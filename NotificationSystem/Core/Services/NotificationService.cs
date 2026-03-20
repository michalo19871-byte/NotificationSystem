using NotificationSystem.Core.Enums;
using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Interfaces;

namespace NotificationSystem.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationQueue _queue;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository repository, INotificationQueue queue, 
                                   ILogger<NotificationService> logger)
        {
            _repository = repository;
            _queue = queue;
            _logger = logger;
        }

        public async Task<Notification> CreateAsync(NotificationType type, string recipient, 
                                              string subject, string body, CancellationToken ct = default)
        {
            var notification = new Notification
            {
                Type = type,
                Recipient = recipient,
                Subject = subject,
                Body = body,
            };

            await _repository.AddAsync(notification, ct);
            await _queue.EnqueueAsync(notification, ct);

            _logger.LogInformation($"Created notification {notification.Id} of type {type} for recipient {recipient}. " +
                                   $"Queue depth: {_queue.Count}");

            return notification;
        }

        public Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default)
            => _repository.GetAllAsync(ct);

        public Task<Notification?> GetStatusAsync(Guid id, CancellationToken ct = default)
            => _repository.GetByIdAsync(id, ct);
    }
}
