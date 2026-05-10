using MassTransit;
using Tailly.Contracts.Messages;
using Tailly.ShopService.Application.Helpers;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Web.BackgroundServices;

public class OrderCompletionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderCompletionBackgroundService> _logger;

    public OrderCompletionBackgroundService(IServiceProvider serviceProvider,
                                            ILogger<OrderCompletionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Completion Background Service started.");

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var orderRepository =
                    scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                var publishEndpoint =
                    scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var orders = await orderRepository.GetAllAsync();

                foreach (var order in orders)
                {
                    if (order.Status == OrderStatus.Cancelled)
                        continue;

                    if (order.CompletionEmailSent)
                        continue;

                    var calculatedStatus = OrderStatusCalculator.Calculate(
                        order.CreatedAt,
                        false
                    );

                    if (calculatedStatus != OrderStatus.Completed)
                        continue;

                    order.Status = OrderStatus.Completed;
                    order.CompletionEmailSent = true;

                    await orderRepository.UpdateAsync(order);

                    await publishEndpoint.Publish(new OrderCompletedEmailRequest
                    {
                        OrderId = order.Id,
                        Email = order.RecipientEmail,
                        FullName = $"{order.RecipientFirstName} {order.RecipientLastName}",
                        OrderNumber = order.Number,
                        TotalPrice = order.TotalPrice,

                        Items = order.Items.Select(x => new OrderEmailItem
                        {
                            Title = x.ProductTitle,
                            Quantity = x.Quantity,
                            Price = x.Price,
                            LineTotal = x.LineTotal
                        }).ToList()
                    });

                    _logger.LogInformation(
                        "Completion email sent for order {OrderId}",
                        order.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while processing completed orders.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}