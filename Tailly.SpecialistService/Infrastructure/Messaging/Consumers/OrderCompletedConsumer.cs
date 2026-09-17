using MassTransit;
using Microsoft.EntityFrameworkCore;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Infrastructure.DataAccess;

namespace Tailly.SpecialistService.Infrastructure.Messaging.Consumers;

public class OrderCompletedConsumer: IConsumer<OrderCompletedMessage>
{
    private readonly SpecialistDbContext _context;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(SpecialistDbContext context,
                                  ILogger<OrderCompletedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<OrderCompletedMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received OrderCompletedMessage: OrderId={OrderId}, SpecialistId={SpecialistId}",
            message.OrderId,
            message.SpecialistId);

        var specialist = await _context.Specialists
            .FirstOrDefaultAsync(
                x => x.Id == message.SpecialistId);

        if (specialist == null)
        {
            _logger.LogWarning(
                "Specialist not found: {SpecialistId}",
                message.SpecialistId);

            return;
        }

        specialist.CompletedOrdersCount += 1;

        if (message.IsRepeatOrder)
        {
            specialist.RepeatOrdersCount += 1;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "CompletedOrdersCount updated for SpecialistId={SpecialistId}",
            message.SpecialistId);
    }
}