using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.DataAccess;

namespace Tailly.AuthService.Configurations.Extensions
{
    public static class DatabaseMigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            try
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<AuthDbContext>();

                await dbContext.Database.MigrateAsync();
            }
            catch (Exception exception)
            {
                var logger = scope.ServiceProvider
                    .GetRequiredService<ILogger<AuthDbContext>>();

                logger.LogError(exception, "Error during database migration.");

                throw;
            }
        }
    }
}
