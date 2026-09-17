using MassTransit;
using Tailly.AuthService.Infrastructure.Service;
using Tailly.Contracts.Messages;

namespace Tailly.AuthService.Infrastructure.Messaging.Consumers;

public class OrderCreatedEmailConsumer : IConsumer<OrderCreatedEmailRequest>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OrderCreatedEmailConsumer> _logger;

    public OrderCreatedEmailConsumer(IEmailSender emailSender,
                                     ILogger<OrderCreatedEmailConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEmailRequest> context)
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
                <h2>Your order has been created!</h2>

                <p>Hello, {message.FullName}!</p>

                <p>
                    Your order <strong>#{message.OrderNumber}</strong>
                    has been successfully placed.
                </p>

                <h3>Order items:</h3>

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

                <h3>Total: {message.TotalPrice} ₽</h3>

                <p>Thank you for shopping with Tailly!</p>";

            await _emailSender.SendEmailAsync(
                message.Email,
                "Your order has been created — Tailly",
                emailBody);

            _logger.LogInformation(
                "Order created email successfully sent to {Email} for order {OrderNumber}",
                message.Email,
                message.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send order created email to {Email}",
                message.Email);

            throw;
        }
    }
}