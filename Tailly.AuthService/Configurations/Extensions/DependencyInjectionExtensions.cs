using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Tailly.AuthService.Configurations.Options;
using Tailly.AuthService.DataAccess;
using Tailly.AuthService.Dtos.Common;
using Tailly.AuthService.Repositories;
using Tailly.AuthService.Repositories.Interfaces;
using Tailly.AuthService.Service.Auth;
using Tailly.AuthService.Service.Security;
using Tailly.AuthService.Service.Tokens;
using Tailly.AuthService.Validators;

namespace Tailly.AuthService.Configurations.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddFixedRateLimiter();
        services.AddSwaggerSetup();
        services.AddPostgres(configuration);
        services.AddOptions(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddSecurityAndCore();
        services.AddApplicationRepositories();
        services.AddFluentValidationSetup();

        return services;
    }

    private static IServiceCollection AddPostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        return services;
    }

    private static IServiceCollection AddOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection(nameof(JwtOptions)));

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
                                                           IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtOptions = configuration.GetSection(nameof(JwtOptions))
                                                  .Get<JwtOptions>()!;

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

        return services;
    }

    private static IServiceCollection AddSecurityAndCore(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }

    private static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

    private static IServiceCollection AddFixedRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter
                .Create<HttpContext, string>(context =>
                {
                    string key =
                        context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        key,
                        _ => new FixedWindowRateLimiterOptions()
                        {
                            PermitLimit = 100,
                            QueueLimit = 10,
                            Window = TimeSpan.FromSeconds(10),
                            AutoReplenishment = true,
                        });
                });
            options.AddPolicy("auth", context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0
                    });
            });
            options.AddPolicy("session", context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 2
                    });
            });
            options.OnRejected = async (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

                    context.HttpContext.Response.Headers.Append("X-Limit-Remaining", "0");
                }

                ExceptionResponse response = new()
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = "Too many requests. Please try again later.",
                };

                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = response.StatusCode;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    response,
                    CancellationToken.None);
            };
        });

        return services;
    }

    private static IServiceCollection AddFluentValidationSetup(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginUserRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RefreshTokenRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>();

        return services;
    }

    private static IServiceCollection AddSwaggerSetup(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        return services;
    }
}