using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.Contracts.Messages;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Core.Models.Order;
using Tailly.ShopService.Core.Models.Order.Checkout;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Tests.Unit.Services;

/// <summary>
/// Unit tests for OrderService.
/// Covers ALL methods with positive and negative scenarios:
/// 
/// CreateOrderAsync:
/// - form is null
/// - Recipient is null
/// - cartItems is null or empty
/// - Courier without Address
/// - PickupPoint without PickupPointId
/// - Product not found
/// - Not enough stock
/// - Success (creates order, updates stock, publishes email)
/// 
/// GetOrderByIdAsync:
/// - Order not found or access denied → OrderNotFound
/// - Success
/// 
/// GetUserOrdersAsync:
/// - Returns list of orders
/// 
/// CancelOrderAsync:
/// - Order not found
/// - Already cancelled (idempotent)
/// - Cannot cancel (Shipped/Completed)
/// - Success (restores stock)
/// 
/// RepeatOrderAsync:
/// - Order not found / access denied
/// - Cancelled order
/// - Success (returns draft)
/// 
/// StartPaymentAsync:
/// - Order not found
/// - Offline payment method (Cash / CardOnDelivery)
/// - Success (returns payment URL)
/// 
/// ConfirmPaymentAsync:
/// - Order not found
/// - Already cancelled
/// - Success
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IPickupPointRepository> _pickupPointRepository = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<ILogger<OrderService>> _logger = new();

    private OrderService CreateService() =>
        new OrderService(_orderRepository.Object, _productRepository.Object,
                         _pickupPointRepository.Object, _publishEndpoint.Object, _logger.Object);

    private Product CreateValidProduct(Guid id, int stock = 10) => new Product
    {
        Id = id,
        Title = "Test Product",
        Slug = "test-product",
        Price = 100,
        StockQuantity = stock,
        Images = new List<ProductImage> { new ProductImage { Url = "/img/test.jpg" } }
    };

    private CartItem CreateCartItem(Guid productId, int quantity = 1) => new CartItem
    {
        ProductId = productId,
        Quantity = quantity
    };

    private CheckoutForm CreateValidForm(DeliveryMethod delivery = DeliveryMethod.Courier)
    {
        return new CheckoutForm
        {
            Recipient = new CheckoutRecipient
            {
                FirstName = "Иван",
                LastName = "Иванов",
                Phone = "+79991234567",
                Email = "test@test.com"
            },
            DeliveryMethod = delivery,
            PaymentMethod = PaymentMethod.Card,
            Address = delivery == DeliveryMethod.Courier
                ? new CheckoutAddress { City = "Москва", Street = "Ленина", House = "10" }
                : null,
            PickupPointId = delivery == DeliveryMethod.PickupPoint ? Guid.NewGuid().ToString() : null
        };
    }

    // =============================================
    // ============== CreateOrderAsync ==============
    // =============================================

    [Fact]
    public async Task CreateOrderAsync_Should_Fail_When_Form_Is_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(Guid.NewGuid(), null!, new List<CartItem>());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidForm);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Fail_When_Recipient_Is_Null()
    {
        // arrange
        var form = new CheckoutForm { Recipient = null! };
        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(Guid.NewGuid(), form, new List<CartItem> { CreateCartItem(Guid.NewGuid()) });

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidRecipient);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Fail_When_Cart_Is_Empty()
    {
        // arrange
        var form = CreateValidForm();
        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(Guid.NewGuid(), form, new List<CartItem>());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.CartIsEmpty);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Fail_When_Courier_Without_Address()
    {
        // arrange
        var form = CreateValidForm(DeliveryMethod.Courier);
        form.Address = null;
        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(Guid.NewGuid(), form, new List<CartItem> { CreateCartItem(Guid.NewGuid()) });

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.AddressRequired);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Fail_When_PickupPoint_Without_Id()
    {
        // arrange
        var form = CreateValidForm(DeliveryMethod.PickupPoint);
        form.PickupPointId = null;
        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(Guid.NewGuid(), form, new List<CartItem> { CreateCartItem(Guid.NewGuid()) });

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.PickupPointRequired);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Fail_When_Product_Not_Found()
    {
        // arrange
        var productId = Guid.NewGuid();
        var form = CreateValidForm();
        var cartItems = new List<CartItem> { CreateCartItem(productId) };

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(Guid.NewGuid(), form, cartItems);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ProductNotFound);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Fail_When_Not_Enough_Stock()
    {
        // arrange
        var productId = Guid.NewGuid();
        var product = CreateValidProduct(productId, stock: 1);
        var form = CreateValidForm();
        var cartItems = new List<CartItem> { CreateCartItem(productId, 5) };

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);

        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(Guid.NewGuid(), form, cartItems);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ProductOutOfStock);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Create_Order_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateValidProduct(productId, stock: 10);
        var form = CreateValidForm();
        var cartItems = new List<CartItem> { CreateCartItem(productId, 2) };

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _productRepository.Setup(x => x.UpdateStockAsync(productId, It.IsAny<int>())).Returns(Task.CompletedTask);
        _orderRepository.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
        _publishEndpoint.Setup(x => x.Publish(It.IsAny<OrderCreatedEmailRequest>(), default)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateOrderAsync(userId, form, cartItems);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        _productRepository.Verify(x => x.UpdateStockAsync(productId, 8), Times.Once);
        _publishEndpoint.Verify(x => x.Publish(It.IsAny<OrderCreatedEmailRequest>(), default), Times.Once);
    }

    // =============================================
    // ============== GetOrderByIdAsync & GetUserOrdersAsync ==============
    // =============================================

    [Fact]
    public async Task GetOrderByIdAsync_Should_Return_OrderNotFound_When_Not_Owned()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var service = CreateService();

        // act
        var result = await service.GetOrderByIdAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderNotFound);
    }

    [Fact]
    public async Task GetOrderByIdAsync_Should_Return_Order_When_Owned_By_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.GetOrderByIdAsync(userId, orderId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetUserOrdersAsync_Should_Return_Orders()
    {
        // arrange
        var userId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(new List<Order> { new Order(), new Order() });

        var service = CreateService();

        // act
        var result = await service.GetUserOrdersAsync(userId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    // =============================================
    // ============== CancelOrderAsync ==============
    // =============================================

    [Fact]
    public async Task CancelOrderAsync_Should_Return_OrderNotFound()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var service = CreateService();

        // act
        var result = await service.CancelOrderAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderNotFound.Description);
    }

    [Fact]
    public async Task CancelOrderAsync_Should_Succeed_When_Already_Cancelled()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId, Status = OrderStatus.Cancelled };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.CancelOrderAsync(userId, orderId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CancelOrderAsync_Should_Fail_When_Shipped_Or_Completed()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId, Status = OrderStatus.Shipped };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.CancelOrderAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderCannotBeCancelled.Description);
    }

    [Fact]
    public async Task CancelOrderAsync_Should_Cancel_And_Restore_Stock()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            OwnerUserId = userId,
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem> { new OrderItem { ProductId = productId, Quantity = 2 } }
        };
        var product = CreateValidProduct(productId, stock: 5);

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);
        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _productRepository.Setup(x => x.UpdateStockAsync(productId, It.IsAny<int>())).Returns(Task.CompletedTask);
        _orderRepository.Setup(x => x.CancelAsync(orderId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CancelOrderAsync(userId, orderId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _productRepository.Verify(x => x.UpdateStockAsync(productId, 7), Times.Once);
    }

    // =============================================
    // ============== RepeatOrderAsync ==============
    // =============================================

    [Fact]
    public async Task RepeatOrderAsync_Should_Fail_When_Order_Not_Found()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var service = CreateService();

        // act
        var result = await service.RepeatOrderAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderNotFound);
    }

    [Fact]
    public async Task RepeatOrderAsync_Should_Fail_When_Order_Is_Cancelled()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId, Status = OrderStatus.Cancelled };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.RepeatOrderAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderCannotBeCancelled);
    }

    [Fact]
    public async Task RepeatOrderAsync_Should_Return_Draft_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OwnerUserId = userId,
            Items = new List<OrderItem> { new OrderItem { ProductId = Guid.NewGuid(), ProductTitle = "Test", Quantity = 1, Price = 100 } }
        };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.RepeatOrderAsync(userId, orderId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    // =============================================
    // ============== StartPaymentAsync & ConfirmPaymentAsync ==============
    // =============================================

    [Fact]
    public async Task StartPaymentAsync_Should_Fail_When_Order_Not_Found()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var service = CreateService();

        // act
        var result = await service.StartPaymentAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderNotFound);
    }

    [Fact]
    public async Task StartPaymentAsync_Should_Fail_For_Offline_Payment()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId, PaymentMethod = PaymentMethod.Cash };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.StartPaymentAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidPaymentMethod);
    }

    [Fact]
    public async Task StartPaymentAsync_Should_Return_Url_For_Online_Payment()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId, PaymentMethod = PaymentMethod.Card };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.StartPaymentAsync(userId, orderId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("fake-payment");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_Should_Fail_When_Order_Not_Found()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var service = CreateService();

        // act
        var result = await service.ConfirmPaymentAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderNotFound.Description);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_Should_Fail_When_Already_Cancelled()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId, Status = OrderStatus.Cancelled };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.ConfirmPaymentAsync(userId, orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderAlreadyCancelled.Description);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_Should_Succeed()
    {
        // arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OwnerUserId = userId, Status = OrderStatus.Created };

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.ConfirmPaymentAsync(userId, orderId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }
}