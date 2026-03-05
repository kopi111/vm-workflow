using VMWorkflow.Domain.Entities;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        var user = context.User.Identity?.Name ?? "anonymous";
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();

        _logger.LogInformation("[AUDIT] {Method} {Path} by {User} at {Timestamp}",
            method, path, user, DateTime.UtcNow);

        // Buffer response body so we can read it for error logging
        var originalBody = context.Response.Body;
        var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);
        }
        catch
        {
            // Restore original body before re-throwing so GlobalExceptionHandler can write
            context.Response.Body = originalBody;
            memoryStream.Dispose();
            throw;
        }

        // Read response body for logging
        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
        memoryStream.Dispose();

        var statusCode = context.Response.StatusCode;

        if (statusCode >= 400 && !string.IsNullOrWhiteSpace(responseBody))
        {
            _logger.LogWarning("[AUDIT] {Method} {Path} by {User} responded {StatusCode} — {ResponseBody}",
                method, path, user, statusCode, responseBody);
        }
        else
        {
            _logger.LogInformation("[AUDIT] {Method} {Path} responded {StatusCode}",
                method, path, statusCode);
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
            db.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                User = user,
                HttpMethod = method,
                Path = path,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist audit log entry");
        }
    }
}
