using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Application.Services;
using OnSteroidsApiUser.Domain.Constants.Enums;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Infrastructure.Dapper;
using OnSteroidsApiUser.Infrastructure.Repositories;
using Serilog;
using System.Text;

namespace OnSteroidsApiUser.WebApi.Configuration;

public static class ProgramExtension
{
    public static void AddKestrelConfig(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });
    }

    public static void AddSerilogConfig(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            var jsonLogPath = Path.Combine(logDirectory, "Structured", "log-.json");
            var textLogPath = Path.Combine(logDirectory, "Text", "log-.txt");

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{RequestId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), jsonLogPath, rollingInterval: RollingInterval.Day)
                .WriteTo.File(textLogPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{RequestId}] {Message:lj}{NewLine}{Exception}");
        });
    }

    public static void AddAppConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
    }

    public static void AddControllerConfig(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = MiddlewareHelpers.ReturnValidationResponse;
            });
    }

    public static void AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "OnSteroids User API",
                Version = "v1",
                Description = "API for OnSteroids user authentication and application state persistence"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT Bearer token only (without 'Bearer ' prefix)"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    public static void AddProjectCors(this WebApplicationBuilder builder)
    {
        var allowedOriginsString = builder.Configuration.GetValue<string>("AppSettings:AllowedOrigins");
        var allowedOrigins = string.IsNullOrWhiteSpace(allowedOriginsString) 
            ? Array.Empty<string>() 
            : allowedOriginsString.Split(',').Select(o => o.Trim()).ToArray();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                nameof(CorsEnum._allowFrontend),
                policy => 
                {
                    if (allowedOrigins.Length > 0 && allowedOrigins[0] != "*")
                    {
                        policy.WithOrigins(allowedOrigins);
                    }
                    else
                    {
                        policy.SetIsOriginAllowed(_ => true);
                    }

                    policy.AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("*");
                }
            );
        });
    }

    public static void AddProjectAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration.GetSection("AppSettings:JWTSettings").Get<JWTSettings>()
            ?? new JWTSettings();

        var key = Encoding.UTF8.GetBytes(jwtSettings.Key
            ?? "DefaultVeryLongSecretKeyForOnSteroidsApplication123456789!@#$%^&*()");

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();
    }

    public static void AddProjectServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Infrastructure
        services.AddSingleton<IDapperConnection, DapperConnection>();
        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<ICapsuleRepository, CapsuleRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IVariableRepository, VariableRepository>();
        services.AddScoped<IRequestHistoryRepository, RequestHistoryRepository>();
        services.AddScoped<IScriptRepository, ScriptRepository>();
        services.AddScoped<IWorkspaceStateRepository, WorkspaceStateRepository>();

        // Application Helpers & Services
        services.AddSingleton<IJwtTokenHelper, JwtTokenHelper>();
        services.AddScoped<IAuthDetailsHelper, AuthDetailsHelper>();
        services.AddTransient<IEmailService, EmailService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICapsuleService, CapsuleService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IVariableService, VariableService>();
        services.AddScoped<IRequestHistoryService, RequestHistoryService>();
        services.AddScoped<IScriptService, ScriptService>();
        services.AddScoped<IWorkspaceStateService, WorkspaceStateService>();
    }

    public static void AddProjectValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IAuthService>();
    }

    public const string OtpGeneratePolicy = "OtpGenerate";
    public const string OtpValidatePolicy = "OtpValidate";

    public static void AddProjectRateLimiter(this WebApplicationBuilder builder)
    {
        var globalRateLimit = builder.Configuration
            .GetSection("AppSettings:GlobalRateLimiter")
            .Get<RateLimitPolicySettings>() ?? new RateLimitPolicySettings
            {
                PermitLimit = 300,
                WindowMinutes = 1,
                QueueLimit = 2,
                ErrorMessage = "Too many requests. Please try again after {retryAfterMinutes} minute(s)."
            };

        var otpRateLimit = builder.Configuration
            .GetSection("AppSettings:OtpSettings:RateLimit")
            .Get<OtpRateLimitSettings>() ?? new OtpRateLimitSettings();

        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = Math.Max(1, globalRateLimit.PermitLimit),
                        QueueLimit = Math.Max(0, globalRateLimit.QueueLimit),
                        Window = TimeSpan.FromMinutes(Math.Max(1, globalRateLimit.WindowMinutes))
                    }));

            // OTP generation (send-otp) — per client IP
            options.AddPolicy(OtpGeneratePolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"{OtpGeneratePolicy}:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = Math.Max(1, otpRateLimit.Generate.PermitLimit),
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(Math.Max(1, otpRateLimit.Generate.WindowMinutes))
                    }));

            // OTP validation (verify-otp) — per client IP
            options.AddPolicy(OtpValidatePolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"{OtpValidatePolicy}:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = Math.Max(1, otpRateLimit.Validate.PermitLimit),
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(Math.Max(1, otpRateLimit.Validate.WindowMinutes))
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var http = context.HttpContext;
                var policyName = http.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;

                var policy = policyName switch
                {
                    OtpGeneratePolicy => otpRateLimit.Generate,
                    OtpValidatePolicy => otpRateLimit.Validate,
                    _ => globalRateLimit
                };

                // Work out how long until the client may retry
                TimeSpan retryAfter = TimeSpan.FromMinutes(policy?.WindowMinutes ?? 1);
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var leaseRetryAfter))
                {
                    retryAfter = leaseRetryAfter;
                }
                var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                var retryAfterMinutes = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes));

                var template = policy?.ErrorMessage
                    ?? "Too many requests. Please try again after {retryAfterMinutes} minute(s).";
                var message = template.Replace("{retryAfterMinutes}", retryAfterMinutes.ToString());

                http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                http.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                await http.Response.WriteAsJsonAsync(
                    new BaseErrorResponse("Too Many Requests", "429", message, [message]),
                    cancellationToken);
            };
        });
    }
}
