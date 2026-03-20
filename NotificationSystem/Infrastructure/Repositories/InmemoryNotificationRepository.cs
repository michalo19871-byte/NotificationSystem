using System.Collections.Concurrent;
using NotificationSystem.Core.Entities;
using NotificationSystem.Core.Enums;
using NotificationSystem.Core.Interfaces;

namespace NotificationSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Thread-safe in-memory repository using ConcurrentDictionary.
    /// </summary>
    public sealed class InmemoryNotificationRepository : INotificationRepository
    {
        private readonly ConcurrentDictionary<Guid, Notification> _store = new();

        public Task<Notification> AddAsync(Notification notification, CancellationToken ct = default)
        {
            _store[notification.Id] = notification;
            return Task.FromResult(notification);
        }

        public Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IEnumerable<Notification>>(_store.Values.ToList());

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            _store.TryGetValue(id, out var notification);
            return Task.FromResult(notification);
        }

        public Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken ct = default)
            => Task.FromResult<IEnumerable<Notification>>(_store.Values.Where(n => n.Status == status).ToList());

        public Task UpdateAsync(Notification notification, CancellationToken ct = default)
        {
            // ConcurrentDictionary indexer is atomic — safe to call from multiple threads
            _store[notification.Id] = notification;
            return Task.CompletedTask;
        }
    }
}
