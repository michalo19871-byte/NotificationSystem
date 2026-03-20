using FluentAssertions;
using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;

namespace NotificationSystem.Tests.UnitTests
{
    public class NotificationEntityTests
    {
        private static Notification CreateEmail() => new()
        {
            Type = NotificationType.Email,
            Recipient = "test@example.com",
            Subject = "Test",
            Body = "Body"
        };

        [Fact]
        public void NewNotification_ShouldHave_PendingStatus()
        {
            // Arrange & Act
            var notification = CreateEmail();

            // Assert
            notification.Status.Should().Be(NotificationStatus.Pending);
            notification.RetryCount.Should().Be(0);
            notification.ProcessedAt.Should().BeNull();
            notification.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void MarkAsProcessing_ShouldSet_ProcessingStatus()
        {
            // Arrange
            var notification = CreateEmail();

            // Act
            notification.MarkAsProcessing();

            // Assert
            notification.Status.Should().Be(NotificationStatus.Processing);
        }

        [Fact]
        public void MarkAsSent_ShouldSet_SentStatus_And_ProcessedAt()
        {
            // Arrange
            var notification = CreateEmail();

            // Act
            notification.MarkAsProcessing();
            notification.MarkAsSent();

            // Assert
            notification.Status.Should().Be(NotificationStatus.Sent);
            notification.ProcessedAt.Should().NotBeNull();
            notification.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MarkAsFailed_ShouldSet_FailedStatus_And_IncrementRetryCount()
        {
            // Arrange
            var notification = CreateEmail();

            // Act
            notification.MarkAsProcessing();
            notification.MarkAsFailed("timeout");

            // Assert
            notification.Status.Should().Be(NotificationStatus.Failed);
            notification.ErrorMessage.Should().Be("timeout");
            notification.RetryCount.Should().Be(1);
            notification.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public void RepeatedFailures_ShouldAccumulate_RetryCount()
        {
            // Arrange
            var notification = CreateEmail();

            // Act
            for (int i = 1; i <= 3; i++)
            {
                notification.MarkAsProcessing();
                notification.MarkAsFailed($"error {i}");
            }

            // Assert
            notification.Status.Should().Be(NotificationStatus.Failed);
            notification.ErrorMessage.Should().Be("error 3");
            notification.RetryCount.Should().Be(3);
        }

        [Fact]
        public void ResetToPending_ShouldClear_ErrorMessage_And_RestoreStatus()
        {
            // Arrange
            var notification = CreateEmail();

            // Act
            notification.MarkAsProcessing();
            notification.MarkAsFailed("network error");
            notification.ResetToPending();

            // Assert
            notification.Status.Should().Be(NotificationStatus.Pending);
            notification.ErrorMessage.Should().BeNull();
        }
    }
}
