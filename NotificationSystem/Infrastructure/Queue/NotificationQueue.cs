using System.Threading.Channels;
using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Interfaces;

namespace NotificationSystem.Infrastructure.Queue
{
    /// <summary>
    /// Bounded, thread-safe notification queue backed by System.Threading.Channels.
    /// Provides natural backpressure and efficient async producer/consumer patterns.
    /// </summary>
    public sealed class NotificationQueue : INotificationQueue
    {
        private readonly Channel<Notification> _channel;
        private readonly ILogger<NotificationQueue> _logger;

        public int Count => _channel.Reader.Count;

        public NotificationQueue(ILogger<NotificationQueue> logger, int capacity = 1000)
        {
            _logger = logger;
            _channel = Channel.CreateBounded<Notification>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            });
        }

        public async ValueTask EnqueueAsync(Notification notification, CancellationToken ct = default)
        {
            await _channel.Writer.WriteAsync(notification, ct);
            _logger.LogDebug($"Notification {notification.Id} enqueued. Queue size: {Count}");
        }

        public async ValueTask<Notification?> DequeueAsync(CancellationToken ct = default)
        {
            try
            {
                return await _channel.Reader.ReadAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (ChannelClosedException)
            {
                return null;
            }
        }
    }
}
