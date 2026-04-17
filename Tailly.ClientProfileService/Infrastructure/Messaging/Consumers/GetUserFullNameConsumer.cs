using MassTransit;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.Contracts.Messages;

namespace Tailly.ClientProfileService.Infrastructure.Messaging.Consumers;

public class GetUserFullNameConsumer: IConsumer<GetUserFullNameRequest>
{
    private readonly IClientProfilesService _service;

    public GetUserFullNameConsumer(IClientProfilesService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<GetUserFullNameRequest> context)
    {
        var result = await _service.GetAsync(context.Message.UserId);

        if (result.IsFailure)
            throw new Exception("Profile not found.");

        var profile = result.Value;

        await context.RespondAsync(new GetUserFullNameResponse
        {
            FirstName = profile.FirstName,
            LastName = profile.LastName
        });
    }
}