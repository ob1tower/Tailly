using MassTransit;
using Microsoft.EntityFrameworkCore;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Infrastructure.DataAccess;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Infrastructure.Messaging.Consumers;

public class SpecialistUserLinkedConsumer : IConsumer<SpecialistUserLinked>
{
    private readonly SpecialistDbContext _context;
    private readonly ILogger<SpecialistUserLinkedConsumer> _logger;

    public SpecialistUserLinkedConsumer(SpecialistDbContext context,
                                        ILogger<SpecialistUserLinkedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SpecialistUserLinked> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received SpecialistUserLinked: SpecialistId={SpecialistId}, UserId={UserId}",
            message.SpecialistId, message.UserId);

        var specialist = await _context.Specialists
            .FirstOrDefaultAsync(s => s.Id == message.SpecialistId);

        if (specialist == null)
        {
            _logger.LogWarning("Specialist not found: {SpecialistId}", message.SpecialistId);
            return;
        }

        if (specialist.UserId == Guid.Empty)
        {
            specialist.UserId = message.UserId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated Specialist {SpecialistId} with UserId {UserId}",
                message.SpecialistId, message.UserId);
        }
        else
        {
            _logger.LogInformation("Specialist {SpecialistId} already has UserId", message.SpecialistId);
        }
    }
}