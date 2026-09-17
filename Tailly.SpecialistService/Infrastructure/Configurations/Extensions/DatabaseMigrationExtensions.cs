using Microsoft.EntityFrameworkCore;
using Tailly.SpecialistService.Infrastructure.DataAccess;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SpecialistDbContext>();

            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<SpecialistDbContext>>();

            logger.LogError(exception, "Error during database migration.");

            throw;
        }
    }
}