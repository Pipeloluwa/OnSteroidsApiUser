using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console();
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
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                nameof(CorsEnum._allowFrontend),
                policy => policy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
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

        // Application Helpers & Services
        services.AddSingleton<IJwtTokenHelper, JwtTokenHelper>();
        services.AddScoped<IAuthDetailsHelper, AuthDetailsHelper>();
        services.AddTransient<IEmailService, EmailService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICapsuleService, CapsuleService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IVariableService, VariableService>();
        services.AddScoped<IRequestHistoryService, RequestHistoryService>();
    }

    public static void AddProjectValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IAuthService>();
    }
}
