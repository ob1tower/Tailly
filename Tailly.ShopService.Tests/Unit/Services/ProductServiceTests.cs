using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.Contracts.Messages;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Catalog;
using Tailly.ShopService.Core.Models.Order;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Tests.Unit.Services;

/// <summary>
/// Unit tests for ProductService.
/// Covers all methods with positive and negative scenarios.
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IRequestClient<GetUserFullNameRequest>> _client = new();
    private readonly Mock<ILogger<ProductService>> _logger = new();

    private ProductService CreateService() =>
        new ProductService(_productRepository.Object, _orderRepository.Object, _client.Object, _logger.Object);

    private Product CreateValidProduct(Guid id) => new Product { Id = id, Title = "Test Product" };

    private Order CreateValidCompletedOrder(Guid userId, Guid productId) => new Order
    {
        Id = Guid.NewGuid(),
        OwnerUserId = userId,
        Status = OrderStatus.Completed,
        Items = new List<OrderItem> { new OrderItem { ProductId = productId } }
    };

    // =============================================
    // ============== GetBySlugAsync ==============
    // =============================================

    [Fact]
    public async Task GetBySlugAsync_Should_Return_InvalidSlug_When_Slug_Is_Empty_Or_Whitespace()
    {
        // arrange
        var service = CreateService();

        // act
        var result1 = await service.GetBySlugAsync("", ReviewSortType.Newest);
        var result2 = await service.GetBySlugAsync("   ", ReviewSortType.Newest);

        // assert
        result1.IsFailure.Should().BeTrue();
        result1.Error.Should().Be(ShopErrors.InvalidSlug);

        result2.IsFailure.Should().BeTrue();
        result2.Error.Should().Be(ShopErrors.InvalidSlug);
    }

    [Fact]
    public async Task GetBySlugAsync_Should_Return_ProductNotFound_When_Product_Not_Found()
    {
        // arrange
        _productRepository.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<ReviewSortType>()))
                          .ReturnsAsync((Product?)null);

        var service = CreateService();

        // act
        var result = await service.GetBySlugAsync("non-existing-slug", ReviewSortType.Newest);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ProductNotFound);
    }

    [Fact]
    public async Task GetBySlugAsync_Should_Return_Product_With_Calculated_Rating_And_ReviewsCount()
    {
        // arrange
        var product = new Product
        {
            Reviews = new List<ProductReview>
            {
                new ProductReview { Rating = 5 },
                new ProductReview { Rating = 3 }
            }
        };

        _productRepository.Setup(x => x.GetBySlugAsync("test-product", It.IsAny<ReviewSortType>()))
                          .ReturnsAsync(product);

        var service = CreateService();

        // act
        var result = await service.GetBySlugAsync("test-product", ReviewSortType.Newest);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReviewsCount.Should().Be(2);
        result.Value.Rating.Should().Be(4.0m);
    }

    // =============================================
    // ============== GetByIdsAsync ==============
    // =============================================

    [Fact]
    public async Task GetByIdsAsync_Should_Return_InvalidProductIds_When_Ids_Is_Empty()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetByIdsAsync("");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidProductIds);
    }

    [Fact]
    public async Task GetByIdsAsync_Should_Return_InvalidProductIds_When_All_Ids_Are_Invalid()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetByIdsAsync("abc,123,not-guid");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidProductIds);
    }

    [Fact]
    public async Task GetByIdsAsync_Should_Return_Products_When_Valid_Ids_Provided()
    {
        // arrange
        var validId1 = Guid.NewGuid();
        var validId2 = Guid.NewGuid();
        var idsString = $"{validId1},{validId2}";

        var products = new List<Product> { new Product(), new Product() };

        _productRepository.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(products);

        var service = CreateService();

        // act
        var result = await service.GetByIdsAsync(idsString);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    // =============================================
    // ============== GetCatalogAsync ==============
    // =============================================

    [Fact]
    public async Task GetCatalogAsync_Should_Return_InvalidPriceRange_When_MinPrice_Greater_Than_MaxPrice()
    {
        // arrange
        var filter = new CatalogFilterState
        {
            MinPrice = 500,
            MaxPrice = 100,
            Sort = ProductSort.Newest
        };

        var service = CreateService();

        // act
        var result = await service.GetCatalogAsync(filter);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidPriceRange);
    }

    [Fact]
    public async Task GetCatalogAsync_Should_Return_Catalog_Successfully()
    {
        // arrange
        var filter = new CatalogFilterState
        {
            Page = 1,
            Limit = 10,
            Sort = ProductSort.Newest
        };

        var products = new List<Product> { new Product() };

        _productRepository
            .Setup(x => x.GetCatalogAsync(
                It.IsAny<string?>(),
                It.IsAny<List<string>?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<bool>(),
                It.IsAny<ProductSort>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((products, 50));

        var service = CreateService();

        // act
        var result = await service.GetCatalogAsync(filter);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.products.Should().HaveCount(1);
        result.Value.total.Should().Be(50);
    }

    // =============================================
    // ============== GetCatalogMetaAsync ==============
    // =============================================

    [Fact]
    public async Task GetCatalogMetaAsync_Should_Return_Meta_Data_Successfully()
    {
        // arrange
        var metaResult = (new List<Category>(), 10m, 5000m);
        _productRepository.Setup(x => x.GetCatalogMetaAsync()).ReturnsAsync(metaResult);

        var service = CreateService();

        // act
        var result = await service.GetCatalogMetaAsync();

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== CreateReviewAsync ==============
    // =============================================

    [Fact]
    public async Task CreateReviewAsync_Should_Return_OrderNotFound_When_Order_Not_Found_Or_Not_Belong_To_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var review = new ProductReview { OrderId = Guid.NewGuid() };

        _orderRepository.Setup(x => x.GetByIdAsync(review.OrderId)).ReturnsAsync((Order?)null);

        var service = CreateService();

        // act
        var result = await service.CreateReviewAsync(userId, review);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderNotFound.Description);
    }

    [Fact]
    public async Task CreateReviewAsync_Should_Return_OrderNotCompleted_When_Order_Status_Is_Not_Completed()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = CreateValidCompletedOrder(userId, productId);
        order.Status = OrderStatus.Created;

        var review = new ProductReview { OrderId = order.Id, ProductId = productId };

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.CreateReviewAsync(userId, review);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.OrderNotCompleted.Description);
    }

    [Fact]
    public async Task CreateReviewAsync_Should_Return_ProductNotInOrder_When_Product_Not_In_Order()
    {
        // arrange
        var userId = Guid.NewGuid();
        var order = CreateValidCompletedOrder(userId, Guid.NewGuid());
        var review = new ProductReview { OrderId = order.Id, ProductId = Guid.NewGuid() }; // другой товар

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.CreateReviewAsync(userId, review);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ProductNotInOrder.Description);
    }

    [Fact]
    public async Task CreateReviewAsync_Should_Return_ReviewAlreadyExists_When_User_Already_Reviewed()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = CreateValidCompletedOrder(userId, productId);
        var review = new ProductReview { OrderId = order.Id, ProductId = productId };

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _productRepository.Setup(x => x.HasReviewAsync(userId, order.Id, productId)).ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.CreateReviewAsync(userId, review);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ReviewAlreadyExists.Description);
    }

    [Fact]
    public async Task CreateReviewAsync_Should_Create_Review_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = CreateValidCompletedOrder(userId, productId);
        var review = new ProductReview
        {
            OrderId = order.Id,
            ProductId = productId,
            Text = "Отличный товар!"
        };

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _productRepository.Setup(x => x.HasReviewAsync(userId, order.Id, productId)).ReturnsAsync(false);

        var response = new Mock<Response<GetUserFullNameResponse>>();
        response.SetupGet(x => x.Message).Returns(new GetUserFullNameResponse
        {
            FirstName = "Иван",
            LastName = "Иванов"
        });

        _client.Setup(x => x.GetResponse<GetUserFullNameResponse>(
                It.IsAny<GetUserFullNameRequest>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<RequestTimeout>()))
            .ReturnsAsync(response.Object);

        _productRepository.Setup(x => x.AddReviewAsync(It.IsAny<ProductReview>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateReviewAsync(userId, review);

        // assert
        result.IsSuccess.Should().BeTrue();

        _client.Verify(x => x.GetResponse<GetUserFullNameResponse>(
            It.IsAny<GetUserFullNameRequest>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<RequestTimeout>()), Times.Once);

        _productRepository.Verify(x => x.AddReviewAsync(It.IsAny<ProductReview>()), Times.Once);
    }

    // =============================================
    // ============== ReplyToReviewAsync ==============
    // =============================================

    [Fact]
    public async Task ReplyToReviewAsync_Should_Return_ReviewNotFound_When_Review_Does_Not_Exist()
    {
        // arrange
        _productRepository.Setup(x => x.GetReviewByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var service = CreateService();

        // act
        var result = await service.ReplyToReviewAsync(Guid.NewGuid(), "Ответ на отзыв");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ReviewNotFound.Description);
    }

    [Fact]
    public async Task ReplyToReviewAsync_Should_Return_ReviewAlreadyHasReply()
    {
        // arrange
        var existingReview = new ProductReview { Reply = new ProductReviewReply() };
        _productRepository.Setup(x => x.GetReviewByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingReview);

        var service = CreateService();

        // act
        var result = await service.ReplyToReviewAsync(Guid.NewGuid(), "Ответ");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.ReviewAlreadyHasReply.Description);
    }

    [Fact]
    public async Task ReplyToReviewAsync_Should_Add_Reply_Successfully()
    {
        // arrange
        var review = new ProductReview();
        _productRepository.Setup(x => x.GetReviewByIdAsync(It.IsAny<Guid>())).ReturnsAsync(review);
        _productRepository.Setup(x => x.AddReviewReplyAsync(It.IsAny<Guid>(), It.IsAny<ProductReviewReply>()))
                          .Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ReplyToReviewAsync(Guid.NewGuid(), "Спасибо за отзыв!");

        // assert
        result.IsSuccess.Should().BeTrue();
        _productRepository.Verify(x => x.AddReviewReplyAsync(It.IsAny<Guid>(), It.IsAny<ProductReviewReply>()), Times.Once);
    }
}