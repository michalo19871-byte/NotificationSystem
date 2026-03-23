using Microsoft.AspNetCore.Mvc;
using NotificationSystem.API.DTOs;
using NotificationSystem.Core.Interfaces;

namespace NotificationSystem.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Produces("application/json")]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        /// <summary>Submits a new notification for async processing.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationMapper.NotificationResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, CancellationToken ct)
        {
            var notification = await _service.CreateAsync(
                request.Type,
                request.Recipient,
                request.Subject,
                request.Body,
                ct
            );

            var response = NotificationMapper.ToResponse(notification);
            return AcceptedAtAction(nameof(GetById), new { id = notification.Id }, response);
        }

        /// <summary>Returns all notifications.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationMapper.NotificationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var notifications = await _service.GetAllAsync(ct);
            return Ok(notifications.Select(NotificationMapper.ToResponse));
        }

        /// <summary>Returns a single notification with its current status.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(NotificationMapper.NotificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var notification = await _service.GetStatusAsync(id, ct);
            return notification is null
                ? NotFound(new { message = $"Notification {id} not found." })
                : Ok(NotificationMapper.ToResponse(notification));
        }
    }
}
