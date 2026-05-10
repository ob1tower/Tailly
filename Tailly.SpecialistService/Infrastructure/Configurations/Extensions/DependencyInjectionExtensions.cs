using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Tailly.SpecialistService.Application.Dtos.Common;
using Tailly.SpecialistService.Application.Service;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Application.Service.Security;
using Tailly.SpecialistService.Application.Validators.SpecialistApplication;
using Tailly.SpecialistService.Application.Validators.SpecialistProfile;
using Tailly.SpecialistService.Infrastructure.Configurations.Constants;
using Tailly.SpecialistService.Infrastructure.Configurations.Options;
using Tailly.SpecialistService.Infrastructure.DataAccess;
using Tailly.SpecialistService.Infrastructure.Messaging.Consumers;
using Tailly.SpecialistService.Infrastructure.Repositories;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
                                                      IConfiguration configuration)
    {
        services.AddJsonSettings();
        services.AddControllers();
        services.AddFixedRateLimiter();
        services.AddSwaggerSetup();
        services.AddRedis(configuration);
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
        services.AddDbContext<SpecialistDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Postgres)));

        return services;
    }

    private static IServiceCollection AddJsonSettings(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull);

        services.ConfigureHttpJsonOptions(o =>
            o.SerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull);

        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services,
                                                 IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection("JwtConfig"));

        services.Configure<ApiSettings>(
            configuration.GetSection("ApiSettings"));

        return services;
    }

    private static IServiceCollection AddRedis(this IServiceCollection services,
                                               IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString(ConnectionStrings.Redis);
        });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString(ConnectionStrings.Redis)!));

        return services;
    }

    private static IServiceCollection AddSecurityAndCore(this IServiceCollection services)
    {
        services.AddScoped<ISpecialistApplicationService, SpecialistApplicationService>();
        services.AddScoped<ISpecialistsService, SpecialistsService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ISpecialistProfileService, SpecialistProfileService>();
        services.AddScoped<SpecialistTemporaryPasswordService>();

        return services;
    }

    private static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISpecialistApplicationRepository, SpecialistApplicationRepository>();
        services.AddScoped<ISpecialistRepository, SpecialistRepository>();

        return services;
    }

    private static IServiceCollection AddFluentValidationSetup(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateSpecialistApplicationRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<AssignInterviewRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RejectApplicationRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ApproveApplicationRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateSpecialistMainInfoRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateSpecialistDetailsRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateServiceRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateServiceRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ReviewReplyRequestValidator>();

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

            options.AddPolicy("specialist-application", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"specialist-app:{ip}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("specialist-actions", context =>
            {
                var specialistId = context.User.FindFirstValue("specialistId")
                                   ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"specialist:{specialistId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("admin-actions", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"admin:{userId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
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
            x.AddConsumer<SpecialistUserLinkedConsumer>();
            x.AddConsumer<ReviewCreatedConsumer>();
            x.AddConsumer<OrderCompletedConsumer>();
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
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier,
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

    private static IServiceCollection AddSwaggerSetup(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename));

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Tailly Specialist API",
                Version = "v1"
            });

            options.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Description = "Enter 'Bearer' [space] and then your valid token."
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