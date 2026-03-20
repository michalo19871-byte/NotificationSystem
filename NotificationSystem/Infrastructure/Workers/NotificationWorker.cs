using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Interfaces;

namespace NotificationSystem.Infrastructure.Workers
{
    /// <summary>
    /// Hosted background service that drains the notification queue concurrently.
    /// Uses a configurable degree of parallelism (semaphore) to prevent thread explosion
    /// while still processing multiple notifications simultaneously.
    /// </summary>
    public sealed class NotificationWorker : BackgroundService
    {
        private const int MaxConcurrency = 4;

        private readonly INotificationQueue _queue;
        private readonly INotificationRepository _repository;
        private readonly IEnumerable<INotificationSender> _senders;
        private readonly ILogger<NotificationWorker> _logger;
        private readonly SemaphoreSlim _semaphore = new(MaxConcurrency, MaxConcurrency);

        public NotificationWorker(INotificationQueue queue, INotificationRepository repository, 
                                  IEnumerable<INotificationSender> senders, ILogger<NotificationWorker> logger)
        {
            _queue = queue;
            _repository = repository;
            _senders = senders;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var processingTasks = new List<Task>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var notification = await _queue.DequeueAsync(stoppingToken);
                if (notification == null) continue;

                await _semaphore.WaitAsync(stoppingToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessNotificationAsync(notification, stoppingToken);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, stoppingToken);

                processingTasks.Add(task);

                // Clean up completed tasks periodically to avoid unbounded list growth
                processingTasks.RemoveAll(t => t.IsCompleted);
            }

            // Drain remaining tasks on graceful shutdown
            await Task.WhenAll(processingTasks);
            _logger.LogInformation("NotificationWorker stopped.");
        }

        private async Task ProcessNotificationAsync(Notification notification, CancellationToken ct)
        {
            _logger.LogInformation($"Processing notification {notification.Id} (type: {notification.Type})");

            notification.MarkAsProcessing();
            await _repository.UpdateAsync(notification, ct);

            var sender = _senders.FirstOrDefault(s => s.SupportedType == notification.Type);
            if (sender == null)
            {
                var error = $"No sender registered for type '{notification.Type}'.";
                _logger.LogError(error);
                notification.MarkAsFailed(error);
                await _repository.UpdateAsync(notification, ct);
                return;
            }

            try
            {
                await sender.SendAsync(notification, ct);
                notification.MarkAsSent();
                _logger.LogInformation($"Notification {notification.Id} sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification {notification.Id}.");
                notification.MarkAsFailed(ex.Message);
            }
            finally
            {
                await _repository.UpdateAsync(notification, ct);
            }
        }
    }
}
