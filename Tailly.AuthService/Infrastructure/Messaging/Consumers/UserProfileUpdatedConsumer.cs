using MassTransit;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;
using Tailly.Contracts.Messages;

namespace Tailly.AuthService.Infrastructure.Messaging.Consumers;

public class UserProfileUpdatedConsumer : IConsumer<UserProfileUpdatedMessage>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger<UserProfileUpdatedConsumer> _logger;

    public UserProfileUpdatedConsumer(IUsersRepository usersRepository,
                                      ILogger<UserProfileUpdatedConsumer> logger)
    {
        _usersRepository = usersRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserProfileUpdatedMessage> context)
    {
        var message = context.Message;

        try
        {
            _logger.LogInformation(
                "Received UserProfileUpdated for UserId: {UserId}",
                message.UserId);

            var user = await _usersRepository.GetByIdAsync(message.UserId);

            if (user == null)
            {
                _logger.LogWarning("User not found for UserProfileUpdated: {UserId}", message.UserId);
                return;
            }

            if (message.FirstName != null)
                user.FirstName = message.FirstName;

            if (message.LastName != null)
                user.LastName = message.LastName;

            if (message.MiddleName != null)
                user.MiddleName = message.MiddleName;

            if (message.SpecialistSlug != null)
                user.SpecialistSlug = message.SpecialistSlug;

            await _usersRepository.UpdateAsync(user);

            _logger.LogInformation("User profile updated successfully for UserId: {UserId}", message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process UserProfileUpdated for UserId: {UserId}",
                message.UserId);

            throw;
        }
    }
}