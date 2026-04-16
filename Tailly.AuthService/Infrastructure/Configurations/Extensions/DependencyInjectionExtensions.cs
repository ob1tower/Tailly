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
using Tailly.AuthService.Application.Dtos.Common;
using Tailly.AuthService.Application.Service.Auth.Common;
using Tailly.AuthService.Application.Service.Auth.Login;
using Tailly.AuthService.Application.Service.Auth.PasswordRecovery;
using Tailly.AuthService.Application.Service.Auth.Registration;
using Tailly.AuthService.Application.Service.Auth.Security;
using Tailly.AuthService.Application.Service.Auth.Token;
using Tailly.AuthService.Application.Service.Claims;
using Tailly.AuthService.Application.Service.Security;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Security.Otp;
using Tailly.AuthService.Application.Service.Tokens;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Application.Validators.Auth;
using Tailly.AuthService.Application.Validators.Auth.PasswordRecovery;
using Tailly.AuthService.Application.Validators.Auth.Register;
using Tailly.AuthService.Application.Validators.Security;
using Tailly.AuthService.Infrastructure.Configurations.Constants;
using Tailly.AuthService.Infrastructure.Configurations.Options;
using Tailly.AuthService.Infrastructure.DataAccess;
using Tailly.AuthService.Infrastructure.Messaging.Consumers;
using Tailly.AuthService.Infrastructure.Repositories;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;
using Tailly.AuthService.Infrastructure.Service;
using Tailly.AuthService.Web.BackgroundServices;

namespace Tailly.AuthService.Infrastructure.Configurations.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
                                                      IConfiguration configuration)
    {
        services.AddJsonSettings();
        services.AddFixedRateLimiter();
        services.AddSwaggerSetup();
        services.AddRedis(configuration);
        services.AddPostgres(configuration);
        services.AddOptions(configuration);
        services.AddJwtAuthentication();
        services.AddSecurityAndCore();
        services.AddFluentValidationSetup();
        services.AddApplicationRepositories();
        services.AddRabbitMq(configuration);

        return services;
    }

    private static IServiceCollection AddPostgres(this IServiceCollection services,
                                                  IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
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

    private static IServiceCollection AddOptions(this IServiceCollection services,
                                                 IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection("JwtConfig"));

        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));

        services.Configure<SecurityOptions>(
            configuration.GetSection("Security"));

        services.Configure<RabbitMqSettings>(
            configuration.GetSection("RabbitMq"));

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
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<IUserSecurityService, UserSecurityService>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ClaimProvider>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddScoped<IPendingRegistrationService, PendingRegistrationService>();

        return services;
    }

    private static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

    private static IServiceCollection AddFluentValidationSetup(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterStartRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RefreshTokenRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ChangePasswordRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RequestEmailChangeRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterVerifyRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterCompleteRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ResetPasswordPayloadValidator>();
        services.AddValidatorsFromAssemblyContaining<ConfirmEmailChangeRequestValidator>();

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

            options.AddPolicy("registration", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                var email = context.Items["RateLimitEmail"] as string ?? "unknown";

                var key = $"reg:{email}:{ip}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("verification", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                var email = context.Items["RateLimitEmail"] as string ?? "unknown";

                var key = $"verify:{email}:{ip}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(15),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("login", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                var email = context.Items["RateLimitEmail"] as string ?? "unknown";

                var key = $"login:{email}:{ip}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(15),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("token", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                var key = userId ?? $"token:{ip}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("password-recovery", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                var email = context.Items["RateLimitEmail"] as string ?? "unknown";

                var key = $"recovery:{email}:{ip}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 4,
                        Window = TimeSpan.FromMinutes(10),
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

    private static IServiceCollection AddRabbitMq(this IServiceCollection services,
                                                  IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<EmailConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()
                    ?? throw new InvalidOperationException("RabbitMq configuration is missing.");

                cfg.Host(new Uri($"amqp://{settings.Host}:{settings.Port}"), h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ReceiveEndpoint(settings.Queue, e =>
                {
                    e.ConfigureConsumer<EmailConsumer>(context);

                    e.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromSeconds(5));
                    });
                });
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
            options.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename));

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