using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Tailly.BookingService.Application.Dtos.Common;
using Tailly.BookingService.Application.Service;
using Tailly.BookingService.Application.Service.Interfaces;
using Tailly.BookingService.Application.Validators;
using Tailly.BookingService.Infrastructure.Configurations.Constants;
using Tailly.BookingService.Infrastructure.Configurations.Options;
using Tailly.BookingService.Infrastructure.DataAccess;
using Tailly.BookingService.Infrastructure.Repositories;

namespace Tailly.BookingService.Infrastructure.Configurations.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
                                                      IConfiguration configuration)
    {
        services.AddJsonSettings();
        services.AddFixedRateLimiter();
        services.AddSwaggerSetup();
        services.AddPostgres(configuration);
        services.AddOptions(configuration);
        services.AddJwtAuthentication();
        services.AddSecurityAndCore();
        services.AddFluentValidationSetup();
        services.AddRabbitMq(configuration);
        services.AddApplicationRepositories();

        return services;
    }

    private static IServiceCollection AddPostgres(this IServiceCollection services,
                                                  IConfiguration configuration)
    {
        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Postgres)));

        return services;
    }

    private static IServiceCollection AddJsonSettings(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services,
                                                 IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection("JwtConfig"));

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
            {
                var jwtOptions = jwtOptionsAccessor.Value;

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

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
        services.AddScoped<IServiceOrderService, ServiceOrderService>();
        services.AddScoped<IMediaService, MediaService>();

        services.AddHttpClient("specialist", client =>
        {
            client.BaseAddress = new Uri("http://specialist:8080");
        });

        services.AddHttpClient("client-profile", client =>
        {
            client.BaseAddress = new Uri("http://clientprofile:8080");
        });

        return services;
    }

    private static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
     
        return services;
    }

    private static IServiceCollection AddFluentValidationSetup(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UploadMediaRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateServiceOrderRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LeaveReviewRequestValidator>();

        return services;
    }

    private static IServiceCollection AddFixedRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter
                .Create<HttpContext, string>(context =>
                {
                    var key = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? context.Connection.RemoteIpAddress?.ToString()
                           ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        key,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            QueueLimit = 10,
                            Window = TimeSpan.FromSeconds(10),
                            AutoReplenishment = true
                        });
                });

            options.AddPolicy("service-orders", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"orders:{userId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy("reviews", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"reviews:{userId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0,
                        AutoReplenishment = true
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

                var response = new ExceptionResponse
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = "Too many requests. Please try again later."
                };

                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = response.StatusCode;

                await context.HttpContext.Response.WriteAsJsonAsync(response, CancellationToken.None);
            };
        });

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()
                    ?? throw new InvalidOperationException("RabbitMq configuration is missing.");

                cfg.Host(new Uri($"amqp://{settings.Host}:{settings.Port}"), h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddSwaggerSetup(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

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