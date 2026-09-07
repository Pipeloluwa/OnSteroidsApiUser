using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Features.Helpers;
using System.Net.Mime;
using System.Text.Json;

namespace OnSteroidsApiUser.Application.MiddlewaresAndFilters.Middlewares;

public class Middleware(
    RequestDelegate next,
    ILogger<Middleware> logger
)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<Middleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault()
            ?? context.Request.Headers["Request-Id"].FirstOrDefault()
            ?? context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.TraceIdentifier = requestId;
        context.Response.Headers["X-Request-ID"] = requestId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = requestId }))
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("[{RequestId}] Incoming HTTP {Method} {Path}", requestId, context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, requestId);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "[{RequestId}] Completed HTTP {Method} {Path} with status {StatusCode} in {ElapsedMilliseconds}ms",
                    requestId, context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds
                );
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, string requestId)
    {
        if (!context.Response.HasStarted)
        {
            _logger.LogError(ex, "[{RequestId}] Unhandled exception occurred while processing request {Path}", requestId, context.Request.Path);

            (int statusCode, var error) = BaseResponseHelpers.ReturnServerErrorData("An unexpected error occurred. Please try again later.", [ex.Message]);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
