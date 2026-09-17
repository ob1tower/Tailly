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
using System.Threading.RateLimiting;
using Tailly.ClientProfileService.Application.Dtos.Requests;
using Tailly.ClientProfileService.Application.Service;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Application.Validators;
using Tailly.ClientProfileService.Infrastructure.Configurations.Constants;
using Tailly.ClientProfileService.Infrastructure.Configurations.Options;
using Tailly.ClientProfileService.Infrastructure.DataAccess;
using Tailly.ClientProfileService.Infrastructure.Messaging.Consumers;
using Tailly.ClientProfileService.Infrastructure.Repositories;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Infrastructure.Configurations.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
                                                      IConfiguration configuration)
    {
        services.AddControllers();
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
        services.AddDbContext<ClientProfileDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString(ConnectionStrings.Postgres)));

        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services,
                                                 IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection("JwtConfig"));

        services.Configure<RabbitMqSettings>(
            configuration.GetSection("RabbitMq"));

        return services;
    }

    private static IServiceCollection AddSecurityAndCore(this IServiceCollection services)
    {
        services.AddScoped<IClientProfilesService, ClientProfilesService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IMediaService, MediaService>();

        return services;
    }

    private static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClientProfileRepository, ClientProfileRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IBreedRepository, BreedRepository>();

        return services;
    }

    private static IServiceCollection AddFluentValidationSetup(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UpsertClientProfileValidator>();
        services.AddValidatorsFromAssemblyContaining<UpsertPetValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateClientProfileMainRequest>();
        services.AddValidatorsFromAssemblyContaining<UpdateClientProfileContactsRequest>();
        services.AddValidatorsFromAssemblyContaining<UploadMediaRequestValidator>();

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
                            Window = TimeSpan.FromSeconds(10),
                            QueueLimit = 10,
                            AutoReplenishment = true
                        });
                });

            options.AddPolicy("profile", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? context.Connection.RemoteIpAddress?.ToString()
                             ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"profile:{userId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("pet", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? context.Connection.RemoteIpAddress?.ToString()
                             ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"pet:{userId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("breed", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"breed:{ip}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("media", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? context.Connection.RemoteIpAddress?.ToString()
                             ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"media:{userId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,                 
                        Window = TimeSpan.FromSeconds(60),  
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

                var response = new
                {
                    statusCode = StatusCodes.Status429TooManyRequests,
                    message = "Too many requests. Please try again later."
                };

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsJsonAsync(response);
            };
        });

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<GetUserFullNameConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()
                    ?? throw new InvalidOperationException("RabbitMq configuration is missing.");

                cfg.Host(new Uri($"amqp://{settings.Host}:{settings.Port}"), h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ReceiveEndpoint("client-profile-user-registered", e =>
                {
                    e.ConfigureConsumer<UserRegisteredConsumer>(context);
                });

                cfg.ReceiveEndpoint("client-profile-get-fullname", e =>
                {
                    e.ConfigureConsumer<GetUserFullNameConsumer>(context);
                });
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
                Title = "Tailly Client Profile API",
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