using MassTransit;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Infrastructure.Messaging.Consumers;

public class SpecialistUserLinkedConsumer : IConsumer<SpecialistUserLinked>
{
    private readonly ISpecialistRepository _specialistRepository;
    private readonly ILogger<SpecialistUserLinkedConsumer> _logger;

    public SpecialistUserLinkedConsumer(ISpecialistRepository specialistRepository,
                                        ILogger<SpecialistUserLinkedConsumer> logger)
    {
        _specialistRepository = specialistRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SpecialistUserLinked> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received SpecialistUserLinked: SpecialistId={SpecialistId}, UserId={UserId}",
            message.SpecialistId, message.UserId);

        var specialist = await _specialistRepository.GetByIdAsync(message.SpecialistId);
        if (specialist == null)
        {
            _logger.LogWarning("Specialist not found: {SpecialistId}", message.SpecialistId);
            return;
        }

        specialist.UserId = message.UserId;
        await _specialistRepository.UpdateAsync(specialist);

        _logger.LogInformation("Successfully updated Specialist {SpecialistId} with UserId {UserId}",
            message.SpecialistId, message.UserId);
    }
}