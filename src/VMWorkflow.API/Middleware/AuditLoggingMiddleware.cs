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

        await _next(context);

        var statusCode = context.Response.StatusCode;

        _logger.LogInformation("[AUDIT] {Method} {Path} responded {StatusCode}",
            method, path, statusCode);

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
