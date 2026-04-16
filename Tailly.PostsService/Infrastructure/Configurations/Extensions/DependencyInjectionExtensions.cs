using FluentValidation;
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
using Tailly.PostsService.Application.Dtos.Common;
using Tailly.PostsService.Application.Service;
using Tailly.PostsService.Application.Service.Interfaces;
using Tailly.PostsService.Application.Validators;
using Tailly.PostsService.Infrastructure.Configurations.Options;
using Tailly.PostsService.Infrastructure.Constants;
using Tailly.PostsService.Infrastructure.DataAccess;
using Tailly.PostsService.Infrastructure.Repositories;
using Tailly.PostsService.Infrastructure.Repositories.Interfaces;

namespace Tailly.PostsService.Infrastructure.Configurations.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
                                                      IConfiguration configuration)
    {
        services.AddJsonSettings();
        services.AddFixedRateLimiter();
        services.AddControllers();
        services.AddSwaggerSetup();
        services.AddPostgres(configuration);
        services.AddOptions(configuration);
        services.AddJwtAuthentication();
        services.AddSecurityAndCore();
        services.AddFluentValidationSetup();
        services.AddApplicationRepositories();

        return services;
    }

    private static IServiceCollection AddPostgres(this IServiceCollection services,
                                                  IConfiguration configuration)
    {
        services.AddDbContext<PostDbContext>(options =>
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

        return services;
    }

    private static IServiceCollection AddSecurityAndCore(this IServiceCollection services)
    {
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IBannerService, BannerService>();

        return services;
    }

    private static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IBannerRepository, BannerRepository>();

        return services;
    }

    private static IServiceCollection AddFluentValidationSetup(this IServiceCollection services)
    {
       services.AddValidatorsFromAssemblyContaining<CreatePostRequestValidator>();
       services.AddValidatorsFromAssemblyContaining<UpdatePostRequestValidator>();

        return services;
    }

    private static IServiceCollection AddFixedRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter
                .Create<HttpContext, string>(context =>
                {
                    var key = context.Connection.RemoteIpAddress?.ToString() ?? "anon";

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

            options.AddPolicy("public", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    ip,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromSeconds(10),
                        QueueLimit = 0
                    });
            });

            options.OnRejected = async (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    statusCode = 429,
                    message = "Too many requests. Please try again later."
                });
            };
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
                Title = "Tailly Post API",
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