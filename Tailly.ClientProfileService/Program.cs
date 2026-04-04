using Tailly.ClientProfileService.Infrastructure.Configurations.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfiguration(builder.Configuration);

WebApplication app = builder.Build();

await app.ApplyMigrationsAsync();

app.Configure();

app.Run();