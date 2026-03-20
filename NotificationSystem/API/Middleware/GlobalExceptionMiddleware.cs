using System.Net;
using System.Text.Json;

namespace NotificationSystem.API.Middleware
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception on {context.Request.Method} {context.Request.Path}");
                await WriteErrorResponseAsync(context, ex);
            }
        }

        private Task WriteErrorResponseAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var payload = JsonSerializer.Serialize(new
            {
                status = 500,
                message = "An unexpected error occurred.",
                detail = ex.Message
            });

            return context.Response.WriteAsync(payload);
        }
    }
}
