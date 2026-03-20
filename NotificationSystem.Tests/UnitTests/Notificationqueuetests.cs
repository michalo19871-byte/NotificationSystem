using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;
using NotificationSystem.Infrastructure.Queue;

namespace NotificationSystem.Tests.UnitTests
{
    public class Notificationqueuetests
    {
        private static NotificationQueue CreateQueue(int capacity = 100)
            => new(NullLogger<NotificationQueue>.Instance, capacity);

        private static Notification MakeNotification() => new()
        {
            Type = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test subject",
            Body = "Test body"
        };

        [Fact]
        public async Task EnqueueAsync_ShouldIncrement_Count()
        {
            // Arrange
            var queue = CreateQueue();

            // Act
            await queue.EnqueueAsync(MakeNotification());

            // Assert
            queue.Count.Should().Be(1);
        }

        [Fact]
        public async Task DequeueAsync_ShouldReturn_EnqueuedItem()
        {
            // Arrange
            var queue = CreateQueue();
            var notification = MakeNotification();
            await queue.EnqueueAsync(notification);

            // Act
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var dequeued = await queue.DequeueAsync(cts.Token);

            // Assert
            dequeued.Should().NotBeNull();
            dequeued!.Id.Should().Be(notification.Id);
        }

        [Fact]
        public async Task DequeueAsync_ShouldReturn_Null_OnCancellation()
        {
            // Arrange
            var queue = CreateQueue();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await queue.DequeueAsync(cts.Token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Queue_ShouldPreserve_FIFO_Order()
        {
            // Arrange
            var queue = CreateQueue();
            var ids = new List<Guid>();

            for (int i = 0; i < 5; i++)
            {
                var notification = MakeNotification();
                ids.Add(notification.Id);
                await queue.EnqueueAsync(notification);
            }

            // Act
            var dequeuedIds = new List<Guid>();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            for (int i = 0; i < 5; i++)
            {
                var n = await queue.DequeueAsync(cts.Token);
                dequeuedIds.Add(n!.Id);
            }

            // Assert
            dequeuedIds.Should().Equal(ids);
        }

        [Fact]
        public async Task Queue_ShouldHandle_ConcurrentProducers()
        {
            // Arrange
            var queue = CreateQueue(capacity: 500);
            const int producers = 10;
            const int itemsPerProducer = 20;

            // Act
            var tasks = Enumerable.Range(0, producers).Select(_ =>
                Task.Run(async () =>
                {
                    for (int i = 0; i < itemsPerProducer; i++)
                        await queue.EnqueueAsync(MakeNotification());
                }));

            await Task.WhenAll(tasks);

            // Assert
            queue.Count.Should().Be(producers * itemsPerProducer);
        }
    }
}
