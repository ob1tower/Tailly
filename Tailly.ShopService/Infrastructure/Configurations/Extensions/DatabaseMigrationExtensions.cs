using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Infrastructure.DataAccess;

namespace Tailly.ShopService.Infrastructure.Configurations.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ShopDbContext>();

            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<ShopDbContext>>();

            logger.LogError(exception, "Error during database migration.");

            throw;
        }
    }
}