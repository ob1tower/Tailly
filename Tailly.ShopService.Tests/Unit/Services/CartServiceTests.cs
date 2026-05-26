using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Tests.Unit.Services;

/// <summary>
/// Unit tests for CartService.
/// Covers ALL methods with positive and negative scenarios:
/// 
/// GetCartAsync:
/// - Both userId and sessionId are null → CartIdentityRequired
/// - User cart exists → returns calculated cart
/// - User has no cart → creates new cart and returns it
/// - Guest cart exists (sessionId) → returns calculated cart
/// - Guest has no cart → creates new cart and returns it
/// 
/// AddItemAsync:
/// - Product not found → ProductNotFound
/// - Item already exists in cart → ItemAlreadyExists
/// - Success → item added, repository called
/// 
/// UpdateItemAsync:
/// - Item not found in cart → ItemNotFound
/// - Success → quantity updated
/// 
/// RemoveItemAsync:
/// - Item not found → ItemNotFound
/// - Success → item removed
/// 
/// ClearCartAsync:
/// - Success → cart cleared
/// 
/// HandleCartAfterLogin:
/// - sessionId is null → does nothing
/// - Guest cart is null → does nothing
/// - merge = true, user has no cart → assigns guest cart to user
/// - merge = true, user already has cart → merges carts + deletes guest
/// - merge = false → deletes guest cart
/// </summary>
public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<ILogger<CartService>> _logger = new();

    private CartService CreateService() =>
        new CartService(_cartRepository.Object, _productRepository.Object, _logger.Object);

    private Cart CreateValidCart(Guid id, Guid? userId = null, Guid? sessionId = null) => new Cart
    {
        Id = id,
        UserId = userId,
        SessionId = sessionId,
        Items = new List<CartItem>()
    };

    private CartItem CreateValidCartItem(Guid productId, int quantity = 1) => new CartItem
    {
        ProductId = productId,
        ProductTitle = "Test Product",
        ProductSlug = "test-product",
        Price = 100,
        Quantity = quantity
    };

    private Product CreateValidProduct(Guid id) => new Product
    {
        Id = id,
        Title = "Test Product",
        Slug = "test-product",
        Price = 100,
        Images = new List<ProductImage> { new ProductImage { Url = "/images/test.jpg" } }
    };

    // =============================================
    // ============== GetCartAsync ==============
    // =============================================

    [Fact]
    public async Task GetCartAsync_Should_Return_CartIdentityRequired_When_Both_Ids_Are_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetCartAsync(null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.CartIdentityRequired);
    }

    [Fact]
    public async Task GetCartAsync_Should_Return_Existing_User_Cart()
    {
        // arrange
        var userId = Guid.NewGuid();
        var cart = CreateValidCart(Guid.NewGuid(), userId);
        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);

        var service = CreateService();

        // act
        var result = await service.GetCartAsync(userId, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCartAsync_Should_Create_New_Cart_When_User_Has_No_Cart()
    {
        // arrange
        var userId = Guid.NewGuid();
        var newCart = CreateValidCart(Guid.NewGuid(), userId);
        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((Cart?)null);
        _cartRepository.Setup(x => x.CreateAsync(userId, null)).ReturnsAsync(newCart);

        var service = CreateService();

        // act
        var result = await service.GetCartAsync(userId, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        _cartRepository.Verify(x => x.CreateAsync(userId, null), Times.Once);
    }

    [Fact]
    public async Task GetCartAsync_Should_Return_Existing_Guest_Cart()
    {
        // arrange
        var sessionId = Guid.NewGuid();
        var cart = CreateValidCart(Guid.NewGuid(), null, sessionId);
        _cartRepository.Setup(x => x.GetBySessionIdAsync(sessionId)).ReturnsAsync(cart);

        var service = CreateService();

        // act
        var result = await service.GetCartAsync(null, sessionId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetCartAsync_Should_Create_New_Guest_Cart_When_Session_Has_No_Cart()
    {
        // arrange
        var sessionId = Guid.NewGuid();
        var newCart = CreateValidCart(Guid.NewGuid(), null, sessionId);
        _cartRepository.Setup(x => x.GetBySessionIdAsync(sessionId)).ReturnsAsync((Cart?)null);
        _cartRepository.Setup(x => x.CreateAsync(null, sessionId)).ReturnsAsync(newCart);

        var service = CreateService();

        // act
        var result = await service.GetCartAsync(null, sessionId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _cartRepository.Verify(x => x.CreateAsync(null, sessionId), Times.Once);
    }

    // =============================================
    // ============== AddItemAsync ==============
    // =============================================

    [Fact]
    public async Task AddItemAsync_Should_Return_ProductNotFound_When_Product_Does_Not_Exist()
    {
        // arrange
        var productId = Guid.NewGuid();
        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        var service = CreateService();

        // act
        var result = await service.AddItemAsync(Guid.NewGuid(), null, productId, 1);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ProductNotFound.Description);
    }

    [Fact]
    public async Task AddItemAsync_Should_Return_ItemAlreadyExists_When_Item_Already_In_Cart()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateValidProduct(productId);
        var cart = CreateValidCart(Guid.NewGuid(), userId);
        cart.Items.Add(CreateValidCartItem(productId));

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);

        var service = CreateService();

        // act
        var result = await service.AddItemAsync(userId, null, productId, 1);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ItemAlreadyExists.Description);
    }

    [Fact]
    public async Task AddItemAsync_Should_Add_Item_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateValidProduct(productId);
        var cart = CreateValidCart(Guid.NewGuid(), userId);

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);
        _cartRepository.Setup(x => x.AddItemAsync(cart.Id, It.IsAny<CartItem>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.AddItemAsync(userId, null, productId, 2);

        // assert
        result.IsSuccess.Should().BeTrue();
        _cartRepository.Verify(x => x.AddItemAsync(cart.Id, It.IsAny<CartItem>()), Times.Once);
    }

    // =============================================
    // ============== UpdateItemAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateItemAsync_Should_Return_ItemNotFound_When_Item_Not_In_Cart()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = CreateValidCart(Guid.NewGuid(), userId);

        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);

        var service = CreateService();

        // act
        var result = await service.UpdateItemAsync(userId, null, productId, 5);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ItemNotFound.Description);
    }

    [Fact]
    public async Task UpdateItemAsync_Should_Update_Item_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = CreateValidCart(Guid.NewGuid(), userId);
        cart.Items.Add(CreateValidCartItem(productId));

        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);
        _cartRepository.Setup(x => x.UpdateItemAsync(cart.Id, productId, 5)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateItemAsync(userId, null, productId, 5);

        // assert
        result.IsSuccess.Should().BeTrue();
        _cartRepository.Verify(x => x.UpdateItemAsync(cart.Id, productId, 5), Times.Once);
    }

    // =============================================
    // ============== RemoveItemAsync ==============
    // =============================================

    [Fact]
    public async Task RemoveItemAsync_Should_Return_ItemNotFound_When_Item_Not_In_Cart()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = CreateValidCart(Guid.NewGuid(), userId);

        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);

        var service = CreateService();

        // act
        var result = await service.RemoveItemAsync(userId, null, productId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ItemNotFound.Description);
    }

    [Fact]
    public async Task RemoveItemAsync_Should_Remove_Item_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = CreateValidCart(Guid.NewGuid(), userId);
        cart.Items.Add(CreateValidCartItem(productId));

        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);
        _cartRepository.Setup(x => x.RemoveItemAsync(cart.Id, productId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.RemoveItemAsync(userId, null, productId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _cartRepository.Verify(x => x.RemoveItemAsync(cart.Id, productId), Times.Once);
    }

    // =============================================
    // ============== ClearCartAsync ==============
    // =============================================

    [Fact]
    public async Task ClearCartAsync_Should_Clear_Cart_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var cart = CreateValidCart(Guid.NewGuid(), userId);

        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);
        _cartRepository.Setup(x => x.ClearAsync(cart.Id)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ClearCartAsync(userId, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        _cartRepository.Verify(x => x.ClearAsync(cart.Id), Times.Once);
    }

    // =============================================
    // ============== HandleCartAfterLogin ==============
    // =============================================

    [Fact]
    public async Task HandleCartAfterLogin_Should_Do_Nothing_When_SessionId_Is_Null()
    {
        // arrange
        var userId = Guid.NewGuid();
        var service = CreateService();

        // act
        await service.HandleCartAfterLogin(userId, null, true);

        // assert
        _cartRepository.Verify(x => x.GetBySessionIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleCartAfterLogin_Should_Do_Nothing_When_Guest_Cart_Is_Null()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _cartRepository.Setup(x => x.GetBySessionIdAsync(sessionId)).ReturnsAsync((Cart?)null);

        var service = CreateService();

        // act
        await service.HandleCartAfterLogin(userId, sessionId, true);

        // assert
        _cartRepository.Verify(x => x.AssignUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleCartAfterLogin_Should_Assign_Guest_Cart_To_User_When_User_Has_No_Cart_And_Merge_Is_True()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var guestCart = CreateValidCart(Guid.NewGuid(), null, sessionId);

        _cartRepository.Setup(x => x.GetBySessionIdAsync(sessionId)).ReturnsAsync(guestCart);
        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((Cart?)null);
        _cartRepository.Setup(x => x.AssignUserAsync(guestCart.Id, userId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        await service.HandleCartAfterLogin(userId, sessionId, true);

        // assert
        _cartRepository.Verify(x => x.AssignUserAsync(guestCart.Id, userId), Times.Once);
    }

    [Fact]
    public async Task HandleCartAfterLogin_Should_Merge_Carts_And_Delete_Guest_When_User_Has_Cart_And_Merge_Is_True()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var guestCart = CreateValidCart(Guid.NewGuid(), null, sessionId);
        var userCart = CreateValidCart(Guid.NewGuid(), userId);

        _cartRepository.Setup(x => x.GetBySessionIdAsync(sessionId)).ReturnsAsync(guestCart);
        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(userCart);
        _cartRepository.Setup(x => x.MergeAsync(userCart.Id, guestCart)).Returns(Task.CompletedTask);
        _cartRepository.Setup(x => x.DeleteBySessionIdAsync(sessionId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        await service.HandleCartAfterLogin(userId, sessionId, true);

        // assert
        _cartRepository.Verify(x => x.MergeAsync(userCart.Id, guestCart), Times.Once);
        _cartRepository.Verify(x => x.DeleteBySessionIdAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task HandleCartAfterLogin_Should_Delete_Guest_Cart_When_Merge_Is_False()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var guestCart = CreateValidCart(Guid.NewGuid(), null, sessionId);

        _cartRepository.Setup(x => x.GetBySessionIdAsync(sessionId)).ReturnsAsync(guestCart);
        _cartRepository.Setup(x => x.DeleteBySessionIdAsync(sessionId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        await service.HandleCartAfterLogin(userId, sessionId, false);

        // assert
        _cartRepository.Verify(x => x.DeleteBySessionIdAsync(sessionId), Times.Once);
        _cartRepository.Verify(x => x.MergeAsync(It.IsAny<Guid>(), It.IsAny<Cart>()), Times.Never);
    }
}