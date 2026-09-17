using MassTransit;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Core.Models;
using Tailly.Contracts.Messages;

namespace Tailly.ClientProfileService.Infrastructure.Messaging.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredMessage>
{
    private readonly IClientProfilesService _clientProfileService;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IClientProfilesService clientProfileService,
                                  ILogger<UserRegisteredConsumer> logger)
    {
        _clientProfileService = clientProfileService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredMessage> context)
    {
        var message = context.Message;

        try
        {
            var profile = new ClientProfile
            {
                UserId = message.UserId,
                FirstName = message.FirstName,
                LastName = message.LastName,
                MiddleName = message.MiddleName,
                Phone = string.Empty,
                City = message.CityName ?? string.Empty,
                CityId = message.CityId,
                AvatarUrl = null
            };

            var result = await _clientProfileService.CreateAsync(message.UserId, profile);

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to create/update profile for user {UserId}. Error: {Error}",
                    message.UserId,
                    result.Error ?? "Unknown error");

                return;
            }

            _logger.LogInformation("Profile synced for user {UserId}", message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating profile for user {UserId}", message.UserId);
        }
    }
}