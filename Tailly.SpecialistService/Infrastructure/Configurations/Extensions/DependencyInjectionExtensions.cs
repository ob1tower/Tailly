using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Tailly.SpecialistService.Application.Service;
using Tailly.SpecialistService.Application.Service.Interfaces;
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
        //services.AddFixedRateLimiter();
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

        services.Configure<RabbitMqSettings>(
            configuration.GetSection("RabbitMq"));

        return services;
    }

    private static IServiceCollection AddSecurityAndCore(this IServiceCollection services)
    {
        services.AddScoped<ISpecialistApplicationService, SpecialistApplicationService>();
        services.AddScoped<ISpecialistsService, SpecialistsService>();
        services.AddScoped<IMediaService, MediaService>();

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
        services.AddValidatorsFromAssemblyContaining<GetSpecialistBySlugRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateSpecialistApplicationRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<AssignInterviewRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RejectApplicationRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<AttachSpecialistAccountRequestValidator>();

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SpecialistUserLinkedConsumer>();
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