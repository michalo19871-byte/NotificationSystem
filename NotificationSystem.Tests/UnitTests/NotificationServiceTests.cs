using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;
using NotificationSystem.Core.Interfaces;
using NotificationSystem.Core.Services;

namespace NotificationSystem.Tests.UnitTests
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _repositoryMock = new();
        private readonly Mock<INotificationQueue> _queueMock = new();
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _service = new NotificationService(_repositoryMock.Object, _queueMock.Object, 
                                               NullLogger<NotificationService>.Instance);
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistAndEnqueue_Notification()
        {
            // Arrange
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Notification n, CancellationToken _) => n);

            // Act
            var result = await _service.CreateAsync(
                NotificationType.Email, 
                "user@example.com",
                "Test Subject",
                "Test Body",
                CancellationToken.None
            );

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be(NotificationType.Email);
            result.Status.Should().Be(NotificationStatus.Pending);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
            _queueMock.Verify(q => q.EnqueueAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldAssign_UniqueIds()
        {
            // Arrange
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Notification n, CancellationToken _) => n);

            // Act
            var result1 = await _service.CreateAsync(
                NotificationType.Sms,
                "+48123456789",
                "SMS Subject",
                "SMS Body",
                CancellationToken.None
            );
            var result2 = await _service.CreateAsync(
                NotificationType.Sms,
                "+48987654321",
                "SMS Subject 2",
                "SMS Body 2",
                CancellationToken.None
            );

            // Assert
            result1.Id.Should().NotBe(result2.Id);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturn_AllNotifications()
        {
            // Arrange
            var notifications = new List<Notification>
            {
                new Notification { Type = NotificationType.Email, 
                                   Recipient = "user@example.com",
                                   Subject = "Test", 
                                   Body = "Test Body",
                                 },
                new Notification { Type = NotificationType.Sms, 
                                   Recipient = "+48123456789",
                                   Subject = "SMS Test", 
                                   Body = "SMS Body",
                                 }
            };

            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(notifications);

            // Act
            var result = await _service.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(notifications.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturn_EmptyList_WhenNoNotifications()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<Notification>());
            // Act
            var result = await _service.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStatusAsync_ShouldReturn_Null_WhenNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((Notification?)null);

            // Act
            var result = await _service.GetStatusAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetStatusAsync_ShouldReturn_Notification_WhenFound()
        {
            // Arrange
            var notification = new Notification
            {
                Type = NotificationType.Email,
                Recipient = "user@example.com",
                Subject = "Test",
                Body = "Test Body"
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(notification);

            // Act
            var result = await _service.GetStatusAsync(notification.Id, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(notification.Id);
        }
    }
}
