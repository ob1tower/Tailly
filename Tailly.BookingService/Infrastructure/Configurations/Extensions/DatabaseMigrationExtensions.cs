using Microsoft.EntityFrameworkCore;
using Tailly.BookingService.Infrastructure.DataAccess;

namespace Tailly.BookingService.Infrastructure.Configurations.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<BookingDbContext>();

            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<BookingDbContext>>();

            logger.LogError(exception, "Error during database migration.");

            throw;
        }
    }
}