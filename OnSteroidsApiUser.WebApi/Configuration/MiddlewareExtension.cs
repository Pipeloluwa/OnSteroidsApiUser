using OnSteroidsApiUser.Application.MiddlewaresAndFilters.Middlewares;

namespace OnSteroidsApiUser.WebApi.Configuration;

public static class MiddlewareExtension
{
    public static void UseMiddlewareExtensions(this WebApplication app)
    {
        app.UseMiddleware<Middleware>();
    }
}
