using MassTransit;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Service;

namespace Tailly.AuthService.Infrastructure.Messaging.Consumers;

public class EmailConsumer : IConsumer<SendEmailMessage>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailConsumer> _logger;

    public EmailConsumer(IEmailSender emailSender, ILogger<EmailConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendEmailMessage> context)
    {
        var message = context.Message;

        try
        {
            await _emailSender.SendEmailAsync(
                message.To,
                message.Subject,
                message.Body);

            _logger.LogInformation("Email sent successfully to {To} | MessageId: {MessageId}",
                message.To, context.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. MessageId: {MessageId}. Will be moved to DLQ.",
                message.To, context.MessageId);

            throw;
        }
    }
}