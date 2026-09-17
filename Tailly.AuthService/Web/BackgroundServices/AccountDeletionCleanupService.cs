using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Web.BackgroundServicesl;

public class AccountDeletionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AccountDeletionCleanupService> _logger;

    public AccountDeletionCleanupService(IServiceProvider serviceProvider,
                                         ILogger<AccountDeletionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Account Deletion Cleanup Service started.");

        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
                var tokenRepository = scope.ServiceProvider.GetRequiredService<IAccountDeletionTokenRepository>();

                var rolesDeleted = await usersRepository.RemoveExpiredDeletedRolesAsync();

                if (rolesDeleted > 0)
                {
                    _logger.LogInformation("Successfully removed {Count} expired soft-deleted roles.", rolesDeleted);
                }

                var tokensDeleted = await tokenRepository.RemoveExpiredCountAsync();

                if (tokensDeleted > 0)
                {
                    _logger.LogInformation("Successfully removed {Count} expired account restore tokens.", tokensDeleted);
                }

                if (rolesDeleted == 0 && tokensDeleted == 0)
                {
                    _logger.LogDebug("No expired roles or tokens to clean up at this time.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while performing account deletion cleanup.");
            }

            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }
}