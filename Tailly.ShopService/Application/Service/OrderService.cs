using CSharpFunctionalExtensions;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Core.Models.Order;
using Tailly.ShopService.Core.Models.Order.Checkout;
using Tailly.ShopService.Core.Models.Pickup;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Application.Service;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPickupPointRepository _pickupPointRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository,
                        IProductRepository productRepository,
                        IPickupPointRepository pickupPointRepository,
                        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _pickupPointRepository = pickupPointRepository;
        _logger = logger;
    }

    public async Task<Result<Order, Error>> CreateOrderAsync(Guid userId, CheckoutForm form, List<CartItem> cartItems)
    {
        if (form == null)
        {
            _logger.LogWarning("CreateOrder failed for user {UserId}: CheckoutForm is null", userId);
            return Result.Failure<Order, Error>(ShopErrors.InvalidForm);
        }

        if (form.Recipient == null)
        {
            _logger.LogWarning("CreateOrder failed for user {UserId}: Recipient is null", userId);
            return Result.Failure<Order, Error>(ShopErrors.InvalidRecipient);
        }

        if (cartItems == null || cartItems.Count == 0)
        {
            _logger.LogWarning("CreateOrder failed for user {UserId}: cart is empty", userId);
            return Result.Failure<Order, Error>(ShopErrors.CartIsEmpty);
        }

        if (form.DeliveryMethod == DeliveryMethod.Courier && form.Address == null)
            return Result.Failure<Order, Error>(ShopErrors.AddressRequired);

        if (form.DeliveryMethod == DeliveryMethod.PickupPoint && string.IsNullOrWhiteSpace(form.PickupPointId))
            return Result.Failure<Order, Error>(ShopErrors.PickupPointRequired);

        if (!Enum.IsDefined(typeof(DeliveryMethod), form.DeliveryMethod))
            return Result.Failure<Order, Error>(ShopErrors.InvalidDeliveryMethod);

        if (!Enum.IsDefined(typeof(PaymentMethod), form.PaymentMethod))
            return Result.Failure<Order, Error>(ShopErrors.InvalidPaymentMethod);

        decimal totalPrice = 0m;
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                _logger.LogWarning("CreateOrder failed: product {ProductId} not found", cartItem.ProductId);
                return Result.Failure<Order, Error>(ShopErrors.ProductNotFound);
            }

            var lineTotal = product.Price * cartItem.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductTitle = product.Title,
                ProductSlug = product.Slug,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal,
                ImageUrl = product.Images?.FirstOrDefault()?.Url
            });

            totalPrice += lineTotal;
        }

        PickupPoint? pickupPoint = null;
        if (form.DeliveryMethod == DeliveryMethod.PickupPoint && !string.IsNullOrWhiteSpace(form.PickupPointId))
        {
            if (!Guid.TryParse(form.PickupPointId, out Guid pickupId))
                return Result.Failure<Order, Error>(ShopErrors.PickupPointNotFound);

            pickupPoint = await _pickupPointRepository.GetByIdAsync(pickupId);
            if (pickupPoint == null)
                return Result.Failure<Order, Error>(ShopErrors.PickupPointNotFound);
        }

        OrderAddress? orderAddress = null;
        if (form.Address != null)
        {
            orderAddress = new OrderAddress
            {
                City = form.Address.City,
                Street = form.Address.Street,
                House = form.Address.House,
                Apartment = form.Address.Apartment ?? "",
                Comment = form.Address.Comment ?? ""
            };
        }

        var status = form.PaymentMethod == PaymentMethod.Cash
            ? OrderStatus.PendingPayment
            : OrderStatus.Created;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            Number = OrderNumberGenerator.Generate(),
            Status = status,
            TotalPrice = totalPrice,
            DeliveryMethod = form.DeliveryMethod,
            PaymentMethod = form.PaymentMethod,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow,
            CanBeCancelled = true,

            RecipientFirstName = form.Recipient.FirstName,
            RecipientLastName = form.Recipient.LastName,
            RecipientPhone = form.Recipient.Phone,
            RecipientEmail = form.Recipient.Email,
            TrackingNumber = $"TRK-{Random.Shared.Next(100000, 999999)}",

            PickupPoint = pickupPoint,
            Address = orderAddress,
            Items = orderItems
        };

        await _orderRepository.AddAsync(order);

        _logger.LogInformation("Order {OrderId} successfully created for user {UserId}", order.Id, userId);

        return Result.Success<Order, Error>(order);
    }

    public async Task<Result<Order, Error>> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null || order.OwnerUserId != userId)
            return Result.Failure<Order, Error>(ShopErrors.OrderNotFound);

        return Result.Success<Order, Error>(order);
    }

    public async Task<Result<List<Order>, Error>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return Result.Success<List<Order>, Error>(orders);
    }

    public async Task<Result> CancelOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null || order.OwnerUserId != userId)
            return Result.Failure(ShopErrors.OrderNotFound.Description);

        if (!order.CanBeCancelled)
            return Result.Failure(ShopErrors.OrderCannotBeCancelled.Description);

        await _orderRepository.CancelAsync(orderId);

        _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);
        return Result.Success();
    }

    public async Task<Result<OrderRepeatCheckoutDraft, Error>> RepeatOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null || order.OwnerUserId != userId)
        {
            _logger.LogWarning("RepeatOrderAsync failed: Order {OrderId} not found or access denied for user {UserId}",
                orderId, userId);
            return Result.Failure<OrderRepeatCheckoutDraft, Error>(ShopErrors.OrderNotFound);
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            _logger.LogWarning("RepeatOrderAsync failed: Cannot repeat cancelled order {OrderId}", orderId);
            return Result.Failure<OrderRepeatCheckoutDraft, Error>(ShopErrors.OrderCannotBeCancelled);
        }

        var draftItems = order.Items.Select(item => new OrderRepeatCheckoutItem
        {
            ProductId = item.ProductId,
            Title = item.ProductTitle,
            Quantity = item.Quantity,
            Price = item.Price,
            ImageUrl = item.ImageUrl
        }).ToList();

        var draft = new OrderRepeatCheckoutDraft
        {
            OrderId = order.Id,
            CreatedAt = DateTime.UtcNow,
            Items = draftItems
        };

        _logger.LogInformation("Repeat checkout draft created for order {OrderId} by user {UserId}. Items count: {Count}",
            orderId, userId, draftItems.Count);

        return Result.Success<OrderRepeatCheckoutDraft, Error>(draft);
    }

    public async Task<Result<string, Error>> StartPaymentAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null || order.OwnerUserId != userId)
            return Result.Failure<string, Error>(ShopErrors.OrderNotFound);

        if (order.Status != OrderStatus.Created && order.Status != OrderStatus.PendingPayment)
            return Result.Failure<string, Error>(ShopErrors.OrderAlreadyPaid);

        if (order.PaymentMethod == PaymentMethod.Cash)
            return Result.Failure<string, Error>(ShopErrors.InvalidPaymentMethod);

        var paymentUrl = $"http://localhost:3000/fake-payment/{orderId}";

        return Result.Success<string, Error>(paymentUrl);
    }

    public async Task<Result> ConfirmPaymentAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null || order.OwnerUserId != userId)
            return Result.Failure(ShopErrors.OrderNotFound.Description);

        if (order.Status == OrderStatus.Paid)
            return Result.Success();

        order.Status = OrderStatus.Paid;
        order.CanBeCancelled = false;

        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Payment confirmed for order {OrderId} by user {UserId}", orderId, userId);

        return Result.Success();
    }
}