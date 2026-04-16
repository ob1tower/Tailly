using Microsoft.EntityFrameworkCore;
using Tailly.PostsService.Infrastructure.DataAccess;

namespace Tailly.PostsService.Infrastructure.Configurations.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<PostDbContext>();

            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<PostDbContext>>();

            logger.LogError(exception, "Error during database migration.");

            throw;
        }
    }
}