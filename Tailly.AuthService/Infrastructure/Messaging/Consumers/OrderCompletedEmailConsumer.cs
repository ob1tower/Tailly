using MassTransit;
using Tailly.AuthService.Infrastructure.Service;
using Tailly.Contracts.Messages;

namespace Tailly.AuthService.Infrastructure.Messaging.Consumers;


public class OrderCompletedEmailConsumer : IConsumer<OrderCompletedEmailRequest>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OrderCompletedEmailConsumer> _logger;

    public OrderCompletedEmailConsumer(IEmailSender emailSender,
                                       ILogger<OrderCompletedEmailConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEmailRequest> context)
    {
        var message = context.Message;

        try
        {
            var itemsHtml = string.Join("",
                message.Items.Select(x => $@"
                    <tr>
                        <td>{x.Title}</td>
                        <td>{x.Quantity}</td>
                        <td>{x.Price} ₽</td>
                        <td>{x.LineTotal} ₽</td>
                    </tr>"));

            var emailBody = $@"
                <h2>Your order has been completed!</h2>

                <p>Hello, {message.FullName}!</p>

                <p>
                    Your order <strong>#{message.OrderNumber}</strong>
                    has been successfully completed.
                </p>

                <h3>Completed order items:</h3>

                <table border='1' cellpadding='8' cellspacing='0'>
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th>Quantity</th>
                            <th>Price</th>
                            <th>Total</th>
                        </tr>
                    </thead>

                    <tbody>
                        {itemsHtml}
                    </tbody>
                </table>

                <h3>Total paid: {message.TotalPrice} ₽</h3>

                <p>Thank you for choosing Tailly!</p>";

            await _emailSender.SendEmailAsync(
                message.Email,
                "Your order has been completed — Tailly",
                emailBody);

            _logger.LogInformation(
                "Order completed email successfully sent to {Email} for order {OrderNumber}",
                message.Email,
                message.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send completed order email to {Email}",
                message.Email);

            throw;
        }
    }
}