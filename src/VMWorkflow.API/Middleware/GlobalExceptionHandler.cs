using System.Net;
using System.Text.Json;

namespace VMWorkflow.API.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad argument");
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            if (_env.IsDevelopment())
            {
                await WriteDetailedErrorResponse(context, ex);
            }
            else
            {
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var response = JsonSerializer.Serialize(new { error = message, statusCode = (int)statusCode });
        await context.Response.WriteAsync(response);
    }

    private static async Task WriteDetailedErrorResponse(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var response = JsonSerializer.Serialize(new
        {
            error = ex.Message,
            type = ex.GetType().Name,
            stackTrace = ex.StackTrace,
            inner = ex.InnerException?.Message,
            statusCode = 500
        }, new JsonSerializerOptions { WriteIndented = true });

        await context.Response.WriteAsync(response);
    }
}
