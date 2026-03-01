using Microsoft.EntityFrameworkCore;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Middleware;

public class UserBlockingMiddleware
{
    private readonly RequestDelegate _next;

    public UserBlockingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        var username = context.User.Identity?.Name;

        if (!string.IsNullOrEmpty(username))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

            var user = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is { IsBlocked: true })
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Your account has been blocked. Contact an administrator.",
                    statusCode = 403
                });
                return;
            }
        }

        await _next(context);
    }
}
