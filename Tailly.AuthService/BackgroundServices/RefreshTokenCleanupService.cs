using Tailly.AuthService.Repositories.Interfaces;

namespace Tailly.AuthService.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Refresh Token Cleanup Service started.");

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

                var deletedCount = await repository.RemoveExpiredTokensAsync();

                if (deletedCount > 0)
                {
                    _logger.LogInformation("Successfully cleaned up {Count} expired refresh tokens.", deletedCount);
                }
                else
                {
                    _logger.LogDebug("No expired refresh tokens to clean up.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired refresh tokens.");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}