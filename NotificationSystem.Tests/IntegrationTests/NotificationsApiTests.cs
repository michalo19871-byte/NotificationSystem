using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NotificationSystem.API.DTOs;
using NotificationSystem.Core.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static NotificationSystem.API.DTOs.NotificationMapper;

namespace NotificationSystem.Tests.Integration;

public class NotificationsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotificationsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateNotificationRequest EmailRequest(
        string recipient = "test@example.com",
        string subject = "Hello",
        string body = "World") => new()
        {
            Type = NotificationType.Email,
            Recipient = recipient,
            Subject = subject,
            Body = body
        };

    [Fact]
    public async Task POST_Notifications_ShouldReturn_202_WithPendingStatus()
    {
        var response = await _client.PostAsJsonAsync("/api/notifications", EmailRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var content = await response.Content.ReadAsStringAsync();
        var notification = JsonSerializer.Deserialize<NotificationResponse>(content, JsonOptions);

        notification.Should().NotBeNull();
        notification!.Id.Should().NotBeEmpty();
        notification.Status.Should().Be("Pending");
        notification.Type.Should().Be("Email");
        notification.Recipient.Should().Be("test@example.com");
    }

    [Fact]
    public async Task POST_Notifications_ShouldReturn_LocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/notifications", EmailRequest());

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/notifications/");
    }

    [Fact]
    public async Task GET_Notification_ShouldReturn_200_ForExistingId()
    {
        // Create first
        var createResponse = await _client.PostAsJsonAsync("/api/notifications", EmailRequest());
        var created = JsonSerializer.Deserialize<NotificationResponse>(
            await createResponse.Content.ReadAsStringAsync(), JsonOptions)!;

        // Retrieve by ID
        var getResponse = await _client.GetAsync($"/api/notifications/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = JsonSerializer.Deserialize<NotificationResponse>(
            await getResponse.Content.ReadAsStringAsync(), JsonOptions)!;

        fetched.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GET_Notification_ShouldReturn_404_ForMissingId()
    {
        var response = await _client.GetAsync($"/api/notifications/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_All_ShouldReturn_AllCreatedNotifications()
    {
        await _client.PostAsJsonAsync("/api/notifications", EmailRequest("a@a.com", "S1", "B1"));
        await _client.PostAsJsonAsync("/api/notifications", new CreateNotificationRequest
        {
            Type = NotificationType.Sms,
            Recipient = "+48123456789",
            Subject = "SMS",
            Body = "Text message"
        });

        var response = await _client.GetAsync("/api/notifications");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var all = JsonSerializer.Deserialize<NotificationResponse[]>(
            await response.Content.ReadAsStringAsync(), JsonOptions)!;

        all.Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_Notifications_ShouldReturn_400_ForInvalidPayload()
    {
        var response = await _client.PostAsJsonAsync("/api/notifications", new { });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task NotificationStatus_ShouldEventuallyTransition_ToSentOrFailed()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/notifications", EmailRequest());
        var created = JsonSerializer.Deserialize<NotificationResponse>(
            await createResponse.Content.ReadAsStringAsync(), JsonOptions)!;

        // Poll up to 5 seconds for the worker to process
        NotificationResponse? latest = null;
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(250);
            var resp = await _client.GetAsync($"/api/notifications/{created.Id}");
            latest = JsonSerializer.Deserialize<NotificationResponse>(
                await resp.Content.ReadAsStringAsync(), JsonOptions)!;

            if (latest.Status is "Sent" or "Failed") break;
        }

        latest!.Status.Should().BeOneOf("Sent", "Failed");
    }
}