using NotificationSystem.API.Middleware;
using NotificationSystem.Core.Interfaces;
using NotificationSystem.Core.Services;
using NotificationSystem.Infrastructure.Queue;
using NotificationSystem.Infrastructure.Repositories;
using NotificationSystem.Infrastructure.Senders;
using NotificationSystem.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Notification Processing API",
        Version = "v1",
        Description = "Async notification processing system supporting Email and SMS."
    });
    options.EnableAnnotations();
});

// Core
builder.Services.AddScoped<INotificationService, NotificationService>();

// Infrastructure: Repository
// Singleton so data survives across requests (in-memory store)
builder.Services.AddSingleton<INotificationRepository, InmemoryNotificationRepository>();

// Infrastructure: Queue
// Singleton to share the same channel between the API and the worker
builder.Services.AddSingleton<INotificationQueue, NotificationQueue>();

// Infrastructure: Senders
// Register all senders; worker resolves them via IEnumerable<INotificationSender>
builder.Services.AddSingleton<INotificationSender, EmailSender>();
builder.Services.AddSingleton<INotificationSender, SmsSender>();

// Infrastructure: Background Worker
builder.Services.AddHostedService<NotificationWorker>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Middleware Pipeline
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Make Program accessible from test projects
public partial class Program { }