using Tailly.AuthService.Infrastructure.Configurations.Extensions;
using Tailly.AuthService.Infrastructure.Seed;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfiguration(builder.Configuration);

WebApplication app = builder.Build();

await app.ApplyMigrationsAsync();

await app.SeedSuperAdminAsync();

await app.SeedTestClientAndSpecialistAsync();

app.Configure();

app.Run();