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
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (!context.Response.HasStarted)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request {Path}", context.Request.Path);

            (int statusCode, var error) = BaseResponseHelpers.ReturnServerErrorData("An unexpected error occurred. Please try again later.", [ex.Message]);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
