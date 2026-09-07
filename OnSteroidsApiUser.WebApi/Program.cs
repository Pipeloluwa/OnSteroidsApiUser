using OnSteroidsApiUser.Domain.Constants.Enums;
using OnSteroidsApiUser.WebApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────
builder.AddKestrelConfig();
builder.AddSerilogConfig();
builder.Services.AddAppConfigurationOptions(builder.Configuration);
builder.AddControllerConfig();
builder.Services.AddSwaggerConfig();
builder.AddProjectCors();
builder.AddProjectAuthentication();

// ── Dependency Injection ───────────────────────────────────────
builder.Services.AddProjectValidators();
builder.Services.AddProjectServices();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnSteroids User API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddlewareExtensions();
app.UseCors(nameof(CorsEnum._allowFrontend));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
