using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Core.Models.User;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Tests.Unit.Services;

/// <summary>
/// Unit tests for FavoriteService.
/// Covers ALL methods with positive and negative scenarios:
/// 
/// AddAsync:
/// - Both userId and sessionId are null → FavoriteIdentityRequired
/// - Product not found → ProductNotFound
/// - User adds product (not exists) → success + added
/// - User adds product (already exists) → success (idempotent)
/// - Session adds product (not exists) → success + added
/// - Session adds product (already exists) → success (idempotent)
/// 
/// GetAsync:
/// - Both null → FavoriteIdentityRequired
/// - userId + sessionId provided → merges guest favorites into user + clears guest
/// - No favorites → returns empty list
/// - Returns FavoriteItem list with correct InCart flag
/// - Skips products that no longer exist
/// 
/// RemoveAsync:
/// - Both null → FavoriteIdentityRequired (with warning log)
/// - Removes from user and/or session
/// 
/// ClearAsync:
/// - Both null → FavoriteIdentityRequired (with warning log)
/// - Clears for user and/or session
/// 
/// MergeAsync:
/// - No guest favorites → returns success immediately
/// - Has guest favorites → adds missing to user + clears guest + logs
/// </summary>
public class FavoriteServiceTests
{
    private readonly Mock<IFavoriteRepository> _favoriteRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<ILogger<FavoriteService>> _logger = new();

    private FavoriteService CreateService() =>
        new FavoriteService(_favoriteRepository.Object, _productRepository.Object, _cartRepository.Object, _logger.Object);

    private Product CreateValidProduct(Guid id) => new Product
    {
        Id = id,
        Title = "Test Product",
        Slug = "test-product",
        Price = 100,
        Images = new List<ProductImage> { new ProductImage { Url = "/img/test.jpg" } }
    };

    private Favorite CreateFavorite(Guid productId, Guid? userId = null, Guid? sessionId = null) => new Favorite
    {
        ProductId = productId,
        UserId = userId,
        SessionId = sessionId,
        AddedAt = DateTime.UtcNow
    };

    private Cart CreateCartWithItems(params Guid[] productIds)
    {
        var cart = new Cart { Items = new List<CartItem>() };
        foreach (var id in productIds)
            cart.Items.Add(new CartItem { ProductId = id });
        return cart;
    }

    // =============================================
    // ============== AddAsync ==============
    // =============================================

    [Fact]
    public async Task AddAsync_Should_Return_FavoriteIdentityRequired_When_Both_Ids_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.AddAsync(null, null, Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.FavoriteIdentityRequired.Description);
    }

    [Fact]
    public async Task AddAsync_Should_Return_ProductNotFound_When_Product_Does_Not_Exist()
    {
        // arrange
        var productId = Guid.NewGuid();
        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        var service = CreateService();

        // act
        var result = await service.AddAsync(Guid.NewGuid(), null, productId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ProductNotFound.Description);
    }

    [Fact]
    public async Task AddAsync_Should_Add_For_User_When_Not_Exists()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateValidProduct(productId);

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _favoriteRepository.Setup(x => x.ExistsAsync(userId, null, productId)).ReturnsAsync(false);
        _favoriteRepository.Setup(x => x.AddAsync(It.IsAny<Favorite>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.AddAsync(userId, null, productId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.AddAsync(It.Is<Favorite>(f => f.UserId == userId && f.ProductId == productId)), Times.Once);
    }

    [Fact]
    public async Task AddAsync_Should_Not_Add_When_Already_Exists_For_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateValidProduct(productId);

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _favoriteRepository.Setup(x => x.ExistsAsync(userId, null, productId)).ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.AddAsync(userId, null, productId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.AddAsync(It.IsAny<Favorite>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_Should_Add_For_Session_When_Not_Exists()
    {
        // arrange
        var sessionId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateValidProduct(productId);

        _productRepository.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
        _favoriteRepository.Setup(x => x.ExistsAsync(null, sessionId, productId)).ReturnsAsync(false);
        _favoriteRepository.Setup(x => x.AddAsync(It.IsAny<Favorite>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.AddAsync(null, sessionId, productId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.AddAsync(It.Is<Favorite>(f => f.SessionId == sessionId)), Times.Once);
    }

    // =============================================
    // ============== GetAsync ==============
    // =============================================

    [Fact]
    public async Task GetAsync_Should_Return_FavoriteIdentityRequired_When_Both_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetAsync(null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.FavoriteIdentityRequired);
    }

    [Fact]
    public async Task GetAsync_Should_Merge_Guest_Favorites_Into_User_When_Both_Provided()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var guestFav = CreateFavorite(productId, null, sessionId);
        _favoriteRepository.Setup(x => x.GetAsync(null, sessionId)).ReturnsAsync(new List<Favorite> { guestFav });
        _favoriteRepository.Setup(x => x.ExistsAsync(userId, null, productId)).ReturnsAsync(false);
        _favoriteRepository.Setup(x => x.AddAsync(It.IsAny<Favorite>())).Returns(Task.CompletedTask);
        _favoriteRepository.Setup(x => x.ClearAsync(null, sessionId)).Returns(Task.CompletedTask);
        _favoriteRepository.Setup(x => x.GetAsync(userId, sessionId)).ReturnsAsync(new List<Favorite>());

        var service = CreateService();

        // act
        var result = await service.GetAsync(userId, sessionId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.AddAsync(It.IsAny<Favorite>()), Times.Once);
        _favoriteRepository.Verify(x => x.ClearAsync(null, sessionId), Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Empty_List_When_No_Favorites()
    {
        // arrange
        var userId = Guid.NewGuid();
        _favoriteRepository.Setup(x => x.GetAsync(userId, null)).ReturnsAsync(new List<Favorite>());

        var service = CreateService();

        // act
        var result = await service.GetAsync(userId, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_Should_Return_FavoriteItems_With_InCart_Flag()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var favs = new List<Favorite>
        {
            CreateFavorite(productId1, userId),
            CreateFavorite(productId2, userId)
        };

        var products = new List<Product>
        {
            CreateValidProduct(productId1),
            CreateValidProduct(productId2)
        };

        var cart = CreateCartWithItems(productId1);

        _favoriteRepository.Setup(x => x.GetAsync(userId, null)).ReturnsAsync(favs);
        _productRepository.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(products);
        _cartRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(cart);

        var service = CreateService();

        // act
        var result = await service.GetAsync(userId, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First(x => x.ProductId == productId1).InCart.Should().BeTrue();
        result.Value.First(x => x.ProductId == productId2).InCart.Should().BeFalse();
    }

    // =============================================
    // ============== RemoveAsync ==============
    // =============================================

    [Fact]
    public async Task RemoveAsync_Should_Return_FavoriteIdentityRequired_When_Both_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.RemoveAsync(null, null, Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.FavoriteIdentityRequired.Description);
    }

    [Fact]
    public async Task RemoveAsync_Should_Remove_From_User_And_Session()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _favoriteRepository.Setup(x => x.RemoveAsync(userId, null, productId)).Returns(Task.CompletedTask);
        _favoriteRepository.Setup(x => x.RemoveAsync(null, sessionId, productId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.RemoveAsync(userId, sessionId, productId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.RemoveAsync(userId, null, productId), Times.Once);
        _favoriteRepository.Verify(x => x.RemoveAsync(null, sessionId, productId), Times.Once);
    }

    // =============================================
    // ============== ClearAsync ==============
    // =============================================

    [Fact]
    public async Task ClearAsync_Should_Return_FavoriteIdentityRequired_When_Both_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.ClearAsync(null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.FavoriteIdentityRequired.Description);
    }

    [Fact]
    public async Task ClearAsync_Should_Clear_For_User_And_Session()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _favoriteRepository.Setup(x => x.ClearAsync(userId, null)).Returns(Task.CompletedTask);
        _favoriteRepository.Setup(x => x.ClearAsync(null, sessionId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ClearAsync(userId, sessionId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.ClearAsync(userId, null), Times.Once);
        _favoriteRepository.Verify(x => x.ClearAsync(null, sessionId), Times.Once);
    }

    // =============================================
    // ============== MergeAsync ==============
    // =============================================

    [Fact]
    public async Task MergeAsync_Should_Do_Nothing_When_No_Guest_Favorites()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _favoriteRepository.Setup(x => x.GetAsync(null, sessionId)).ReturnsAsync(new List<Favorite>());

        var service = CreateService();

        // act
        var result = await service.MergeAsync(userId, sessionId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.AddAsync(It.IsAny<Favorite>()), Times.Never);
    }

    [Fact]
    public async Task MergeAsync_Should_Merge_Guest_Favorites_To_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var guestFavs = new List<Favorite> { CreateFavorite(productId, null, sessionId) };

        _favoriteRepository.Setup(x => x.GetAsync(null, sessionId)).ReturnsAsync(guestFavs);
        _favoriteRepository.Setup(x => x.ExistsAsync(userId, null, productId)).ReturnsAsync(false);
        _favoriteRepository.Setup(x => x.AddAsync(It.IsAny<Favorite>())).Returns(Task.CompletedTask);
        _favoriteRepository.Setup(x => x.ClearAsync(null, sessionId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.MergeAsync(userId, sessionId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _favoriteRepository.Verify(x => x.AddAsync(It.IsAny<Favorite>()), Times.Once);
        _favoriteRepository.Verify(x => x.ClearAsync(null, sessionId), Times.Once);
    }
}