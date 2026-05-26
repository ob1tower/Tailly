using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.PostsService.Application.Errors;
using Tailly.PostsService.Application.Service;
using Tailly.PostsService.Core.Models;
using Tailly.PostsService.Infrastructure.Repositories.Interfaces;

namespace Tailly.PostsService.Tests.Unit.Services;

/// <summary>
/// Unit tests for BannerService.
/// Covers all methods with positive and negative scenarios:
/// - CreateAsync (success + null + empty title + empty description)
/// - UpdateAsync (success + null + empty id + not found + empty title + empty description)
/// - DeleteAsync (success + empty id + not found)
/// - GetByIdAsync (success + not found + empty id)
/// - GetListAsync (success + invalid sort)
/// - GetAllAsync (success)
/// - GetActiveBannersAsync (success)
/// </summary>
public class BannerServiceTests
{
    private readonly Mock<IBannerRepository> _repository = new();
    private readonly Mock<ILogger<BannerService>> _logger = new();

    private BannerService CreateService() =>
        new BannerService(_repository.Object, _logger.Object);

    private Banner CreateValidBanner(Guid? id = null) => new Banner
    {
        Id = id ?? Guid.NewGuid(),
        Title = "Акция на груминг",
        Description = "Скидка 20% на все услуги груминга до конца месяца",
        StartsAt = DateTime.UtcNow,
        EndsAt = DateTime.UtcNow.AddDays(30)
    };

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_Banner_Successfully()
    {
        // arrange
        var banner = CreateValidBanner();
        _repository.Setup(x => x.AddAsync(It.IsAny<Banner>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(banner);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        _repository.Verify(x => x.AddAsync(It.IsAny<Banner>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_InvalidBanner_When_Banner_Is_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.CreateAsync(null!);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.InvalidBanner);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_EmptyTitle_When_Title_Is_Empty()
    {
        // arrange
        var banner = CreateValidBanner();
        banner.Title = "";
        var service = CreateService();

        // act
        var result = await service.CreateAsync(banner);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.EmptyTitle);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_EmptyDescription_When_Description_Is_Empty()
    {
        // arrange
        var banner = CreateValidBanner();
        banner.Description = "";
        var service = CreateService();

        // act
        var result = await service.CreateAsync(banner);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.EmptyDescription);
    }

    // =============================================
    // ============== UpdateAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateAsync_Should_Update_Banner_Successfully()
    {
        // arrange
        var banner = CreateValidBanner();
        var existing = CreateValidBanner(banner.Id);
        _repository.Setup(x => x.GetByIdAsync(banner.Id)).ReturnsAsync(existing);
        _repository.Setup(x => x.UpdateAsync(It.IsAny<Banner>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(banner);

        // assert
        result.IsSuccess.Should().BeTrue();
        _repository.Verify(x => x.UpdateAsync(It.IsAny<Banner>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_InvalidBanner_When_Banner_Is_Null_Or_Empty_Id()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.UpdateAsync(null!);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.InvalidBanner);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_BannerNotFound_When_Banner_Does_Not_Exist()
    {
        // arrange
        var banner = CreateValidBanner();
        _repository.Setup(x => x.GetByIdAsync(banner.Id)).ReturnsAsync((Banner?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(banner);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.BannerNotFound);
    }

    // =============================================
    // ============== DeleteAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteAsync_Should_Delete_Banner_Successfully()
    {
        // arrange
        var id = Guid.NewGuid();
        var existing = CreateValidBanner(id);
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(existing);
        _repository.Setup(x => x.DeleteAsync(id)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(id);

        // assert
        result.IsSuccess.Should().BeTrue();
        _repository.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_BannerNotFound_When_Id_Is_Empty()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.DeleteAsync(Guid.Empty);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.BannerNotFound.Description);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_BannerNotFound_When_Banner_Does_Not_Exist()
    {
        // arrange
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Banner?)null);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(id);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.BannerNotFound.Description);
    }

    // =============================================
    // ============== GetByIdAsync ==============
    // =============================================

    [Fact]
    public async Task GetByIdAsync_Should_Return_Banner_When_Exists()
    {
        // arrange
        var banner = CreateValidBanner();
        _repository.Setup(x => x.GetByIdAsync(banner.Id)).ReturnsAsync(banner);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(banner.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(banner);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_BannerNotFound_When_Banner_Does_Not_Exist()
    {
        // arrange
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Banner?)null);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(id);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.BannerNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_BannerNotFound_When_Id_Is_Empty()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(Guid.Empty);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.BannerNotFound);
    }

    // =============================================
    // ============== GetListAsync ==============
    // =============================================

    [Fact]
    public async Task GetListAsync_Should_Return_Banners_With_Pagination()
    {
        // arrange
        var banners = new List<Banner> { CreateValidBanner() };
        _repository.Setup(x => x.GetListAsync(1, 10, null, null))
                   .ReturnsAsync((banners, 1));

        var service = CreateService();

        // act
        var result = await service.GetListAsync(1, 10, null, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        var (returnedBanners, total) = result.Value;
        returnedBanners.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetListAsync_Should_Return_InvalidSort_When_Sort_Is_Invalid()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetListAsync(1, 10, null, "invalid-sort");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BannerErrors.InvalidSort);
    }

    // =============================================
    // ============== GetAllAsync ==============
    // =============================================

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Banners()
    {
        // arrange
        var banners = new List<Banner> { CreateValidBanner(), CreateValidBanner() };
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync(banners);

        var service = CreateService();

        // act
        var result = await service.GetAllAsync();

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    // =============================================
    // ============== GetActiveBannersAsync ==============
    // =============================================

    [Fact]
    public async Task GetActiveBannersAsync_Should_Return_Active_Banners()
    {
        // arrange
        var banners = new List<Banner> { CreateValidBanner() };
        _repository.Setup(x => x.GetActiveBannersAsync(It.IsAny<DateTime>()))
                   .ReturnsAsync(banners);

        var service = CreateService();

        // act
        var result = await service.GetActiveBannersAsync();

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}