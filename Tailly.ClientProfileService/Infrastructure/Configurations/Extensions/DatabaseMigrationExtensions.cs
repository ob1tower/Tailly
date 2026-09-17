using Microsoft.EntityFrameworkCore;
using Tailly.ClientProfileService.Infrastructure.DataAccess;

namespace Tailly.ClientProfileService.Infrastructure.Configurations.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ClientProfileDbContext>();

            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<ClientProfileDbContext>>();

            logger.LogError(exception, "Error during database migration.");

            throw;
        }
    }
}