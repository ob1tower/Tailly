using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service;
using Tailly.ShopService.Core.Models.Pickup;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Tests.Unit.Services;

/// <summary>
/// Unit tests for PickupPointService.
/// Covers all methods with positive and negative scenarios:
/// 
/// GetAllAsync:
/// - Returns list of pickup points
/// - Returns empty list when no pickup points exist
/// 
/// GetByCityAsync:
/// - Empty string / whitespace → InvalidCityName
/// - Null value → InvalidCityName
/// - Valid city name → returns pickup points for that city
/// - City with no pickup points → returns empty list
/// 
/// GetByIdAsync:
/// - Empty Guid (Guid.Empty) → PickupPointNotFound
/// - Pickup point not found in database → PickupPointNotFound
/// - Valid id → returns pickup point successfully
/// </summary>
public class PickupPointServiceTests
{
    private readonly Mock<IPickupPointRepository> _pickupPointRepository = new();
    private readonly Mock<ILogger<PickupPointService>> _logger = new();

    private PickupPointService CreateService() =>
        new PickupPointService(_pickupPointRepository.Object, _logger.Object);

    private PickupPoint CreateValidPickupPoint(Guid id) => new PickupPoint
    {
        Id = id,
        Title = "Пункт выдачи",
        Address = "ул. Ленина, 10, Москва"
    };

    // =============================================
    // ============== GetAllAsync ==============
    // =============================================

    [Fact]
    public async Task GetAllAsync_Should_Return_All_PickupPoints()
    {
        // arrange
        var points = new List<PickupPoint>
        {
            CreateValidPickupPoint(Guid.NewGuid()),
            CreateValidPickupPoint(Guid.NewGuid())
        };

        _pickupPointRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(points);

        var service = CreateService();

        // act
        var result = await service.GetAllAsync();

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Empty_List_When_No_PickupPoints()
    {
        // arrange
        _pickupPointRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PickupPoint>());

        var service = CreateService();

        // act
        var result = await service.GetAllAsync();

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // =============================================
    // ============== GetByCityAsync ==============
    // =============================================

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task GetByCityAsync_Should_Return_InvalidCityName_When_City_Is_Empty_Or_Whitespace(string city)
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetByCityAsync(city);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidCityName);
    }

    [Fact]
    public async Task GetByCityAsync_Should_Return_InvalidCityName_When_City_Is_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetByCityAsync(null!);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.InvalidCityName);
    }

    [Fact]
    public async Task GetByCityAsync_Should_Return_PickupPoints_For_Valid_City()
    {
        // arrange
        var city = "Москва";
        var points = new List<PickupPoint>
        {
            CreateValidPickupPoint(Guid.NewGuid()),
            CreateValidPickupPoint(Guid.NewGuid())
        };

        _pickupPointRepository.Setup(x => x.GetByCityAsync(city)).ReturnsAsync(points);

        var service = CreateService();

        // act
        var result = await service.GetByCityAsync(city);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCityAsync_Should_Return_Empty_List_When_No_PickupPoints_In_City()
    {
        // arrange
        var city = "Неизвестный город";
        _pickupPointRepository.Setup(x => x.GetByCityAsync(city)).ReturnsAsync(new List<PickupPoint>());

        var service = CreateService();

        // act
        var result = await service.GetByCityAsync(city);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // =============================================
    // ============== GetByIdAsync ==============
    // =============================================

    [Fact]
    public async Task GetByIdAsync_Should_Return_PickupPointNotFound_When_Id_Is_Empty()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(Guid.Empty);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.PickupPointNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_PickupPointNotFound_When_PickupPoint_Not_Found()
    {
        // arrange
        var id = Guid.NewGuid();
        _pickupPointRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((PickupPoint?)null);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(id);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ShopErrors.PickupPointNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_PickupPoint_When_Found()
    {
        // arrange
        var id = Guid.NewGuid();
        var point = CreateValidPickupPoint(id);

        _pickupPointRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(point);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(id);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
    }
}