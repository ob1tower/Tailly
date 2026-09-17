using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tailly.SpecialistService.Application.Dtos.Responses.Home;
using Tailly.SpecialistService.Application.Service;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Configurations.Options;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Tests.Unit.Services;

/// <summary>
/// Unit tests for SpecialistsService.
/// Covers ALL methods with positive and negative scenarios:
/// - SearchAsync (success with all parameters + all null parameters + onlyWithReviews = true)
/// - GetFullProfileByIdAsync (success + returns null when not found)
/// - GetFullProfileBySlugAsync (success + returns null when not found)
/// - GetHomeReviewsAsync (success with photo url transformation + empty list + all null parameters)
/// </summary>
public class SpecialistsServiceTests
{
    private readonly Mock<ISpecialistRepository> _repository = new();
    private readonly Mock<IOptions<ApiSettings>> _options = new();
    private readonly Mock<ILogger<SpecialistsService>> _logger = new();

    private SpecialistsService CreateService()
    {
        var apiSettings = new ApiSettings { BaseUrl = "https://api.tailly.ru" };
        _options.Setup(x => x.Value).Returns(apiSettings);

        return new SpecialistsService(_repository.Object, _options.Object, _logger.Object);
    }

    // =============================================
    // ============== SearchAsync ==============
    // =============================================

    [Fact]
    public async Task SearchAsync_Should_Call_Repository_With_All_Parameters_And_Log()
    {
        // arrange
        var expected = new List<Specialist> { new Specialist() };
        _repository.Setup(x => x.SearchAsync(
            "Moscow", "Center", "Grooming", "Dog", 2, true, "rating", 1000m, 5000m, 1, 20))
            .ReturnsAsync(expected);

        var service = CreateService();

        // act
        var result = await service.SearchAsync(
            "Moscow", "Center", "Grooming", "Dog", 2, true, "rating", 1000m, 5000m, 1, 20);

        // assert
        result.Should().BeEquivalentTo(expected);

        _logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Searching specialists")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_Should_Work_With_All_Null_Parameters()
    {
        // arrange
        var expected = new List<Specialist>();
        _repository.Setup(x => x.SearchAsync(null, null, null, null, null, false, null, null, null, 1, 10))
            .ReturnsAsync(expected);

        var service = CreateService();

        // act
        var result = await service.SearchAsync(null, null, null, null, null, false, null, null, null, 1, 10);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_Should_Work_With_OnlyWithReviews_True()
    {
        // arrange
        var expected = new List<Specialist> { new Specialist() };
        _repository.Setup(x => x.SearchAsync(null, null, null, null, null, true, null, null, null, 1, 20))
            .ReturnsAsync(expected);

        var service = CreateService();

        // act
        var result = await service.SearchAsync(null, null, null, null, null, true, null, null, null, 1, 20);

        // assert
        result.Should().HaveCount(1);
    }

    // =============================================
    // ============== GetFullProfileByIdAsync ==============
    // =============================================

    [Fact]
    public async Task GetFullProfileByIdAsync_Should_Return_Specialist()
    {
        // arrange
        var id = Guid.NewGuid();
        var specialist = new Specialist { Id = id };
        _repository.Setup(x => x.GetFullProfileByIdAsync(id, ReviewSortType.Newest))
                   .ReturnsAsync(specialist);

        var service = CreateService();

        // act
        var result = await service.GetFullProfileByIdAsync(id, ReviewSortType.Newest);

        // assert
        result.Should().BeEquivalentTo(specialist);
    }

    [Fact]
    public async Task GetFullProfileByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // arrange
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetFullProfileByIdAsync(id, ReviewSortType.Newest))
                   .ReturnsAsync((Specialist?)null);

        var service = CreateService();

        // act
        var result = await service.GetFullProfileByIdAsync(id, ReviewSortType.Newest);

        // assert
        result.Should().BeNull();
    }

    // =============================================
    // ============== GetFullProfileBySlugAsync ==============
    // =============================================

    [Fact]
    public async Task GetFullProfileBySlugAsync_Should_Return_Specialist()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialist = new Specialist { Slug = slug };
        _repository.Setup(x => x.GetFullProfileBySlugAsync(slug, ReviewSortType.Newest))
                   .ReturnsAsync(specialist);

        var service = CreateService();

        // act
        var result = await service.GetFullProfileBySlugAsync(slug, ReviewSortType.Newest);

        // assert
        result.Should().BeEquivalentTo(specialist);
    }

    [Fact]
    public async Task GetFullProfileBySlugAsync_Should_Return_Null_When_Not_Found()
    {
        // arrange
        var slug = "unknown-slug";
        _repository.Setup(x => x.GetFullProfileBySlugAsync(slug, ReviewSortType.Newest))
                   .ReturnsAsync((Specialist?)null);

        var service = CreateService();

        // act
        var result = await service.GetFullProfileBySlugAsync(slug, ReviewSortType.Newest);

        // assert
        result.Should().BeNull();
    }

    // =============================================
    // ============== GetHomeReviewsAsync ==============
    // =============================================

    [Fact]
    public async Task GetHomeReviewsAsync_Should_Return_Reviews_With_Full_Photo_Urls()
    {
        // arrange
        var reviews = new List<HomeReviewResponse>
        {
            new HomeReviewResponse { PhotoUrls = new List<string> { "/uploads/1.jpg", "/uploads/2.jpg" } },
            new HomeReviewResponse { PhotoUrls = new List<string> { "/uploads/3.jpg" } }
        };

        _repository.Setup(x => x.GetHomeReviewsAsync(5, 10, true, 50, 5))
                   .ReturnsAsync(reviews);

        var service = CreateService();

        // act
        var result = await service.GetHomeReviewsAsync(5, 10, true, 50, 5);

        // assert
        result.Should().HaveCount(2);
        result[0].PhotoUrls.Should().BeEquivalentTo(new List<string>
        {
            "https://api.tailly.ru/uploads/1.jpg",
            "https://api.tailly.ru/uploads/2.jpg"
        });
        result[1].PhotoUrls.Should().BeEquivalentTo(new List<string>
        {
            "https://api.tailly.ru/uploads/3.jpg"
        });
    }

    [Fact]
    public async Task GetHomeReviewsAsync_Should_Return_Empty_List_When_No_Reviews()
    {
        // arrange
        _repository.Setup(x => x.GetHomeReviewsAsync(null, 20, false, 0, 0))
                   .ReturnsAsync(new List<HomeReviewResponse>());

        var service = CreateService();

        // act
        var result = await service.GetHomeReviewsAsync(null, 20, false, 0, 0);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHomeReviewsAsync_Should_Work_With_All_Null_Parameters()
    {
        // arrange
        var reviews = new List<HomeReviewResponse> { new HomeReviewResponse() };
        _repository.Setup(x => x.GetHomeReviewsAsync(null, 10, false, 0, 0))
                   .ReturnsAsync(reviews);

        var service = CreateService();

        // act
        var result = await service.GetHomeReviewsAsync(null, 10, false, 0, 0);

        // assert
        result.Should().HaveCount(1);
    }
}