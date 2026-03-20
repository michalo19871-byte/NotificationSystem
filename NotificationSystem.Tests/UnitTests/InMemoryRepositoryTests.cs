using FluentAssertions;
using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;
using NotificationSystem.Infrastructure.Repositories;

namespace NotificationSystem.Tests.UnitTests
{
    public class InMemoryRepositoryTests
    {
        private static Notification MakeNotification(NotificationType type = NotificationType.Email) => new()
        {
            Type = type,
            Recipient = "test@example.com",
            Subject = "Test subject",
            Body = "Test body"
        };

        [Fact]
        public async Task AddAsync_ShouldStore_Notification()
        {
            // Arrange
            var repository = new InmemoryNotificationRepository();
            var notification = MakeNotification();

            // Act
            await repository.AddAsync(notification);
            var found = await repository.GetByIdAsync(notification.Id);

            // Assert
            found.Should().NotBeNull();
            found!.Id.Should().Be(notification.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_Null_ForMissingId()
        {
            // Arrange
            var repository = new InmemoryNotificationRepository();

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturn_AllStored()
        {
            // Arrange
            var repository = new InmemoryNotificationRepository();

            // Act
            await repository.AddAsync(MakeNotification(NotificationType.Email));
            await repository.AddAsync(MakeNotification(NotificationType.Sms));
            var all = await repository.GetAllAsync();

            // Assert
            all.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldFilter_ByStatus()
        {
            // Arrange
            var repository = new InmemoryNotificationRepository();
            var pending = MakeNotification();
            var processing = MakeNotification();
            processing.MarkAsProcessing();

            // Act
            await repository.AddAsync(pending);
            await repository.AddAsync(processing);
            var pendingList = await repository.GetByStatusAsync(NotificationStatus.Pending);

            // Assert
            pendingList.Should().HaveCount(1);
            pendingList.Single().Id.Should().Be(pending.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReplace_ExistingRecord()
        {
            // Arrange
            var repository = new InmemoryNotificationRepository();
            var notification = MakeNotification();
            await repository.AddAsync(notification);
            notification.MarkAsProcessing();
            notification.MarkAsSent();

            // Act
            await repository.UpdateAsync(notification);
            var updated = await repository.GetByIdAsync(notification.Id);

            // Assert
            updated!.Status.Should().Be(NotificationStatus.Sent);
        }

        [Fact]
        public async Task Repository_ShouldBe_ThreadSafe_UnderConcurrentWrites()
        {
            // Arrange
            var repo = new InmemoryNotificationRepository();
            const int count = 100;
            var tasks = Enumerable.Range(0, count)
                .Select(_ => repo.AddAsync(MakeNotification()));

            // Act
            await Task.WhenAll(tasks);
            var all = await repo.GetAllAsync();

            // Assert
            all.Should().HaveCount(count);
        }
    }
}
