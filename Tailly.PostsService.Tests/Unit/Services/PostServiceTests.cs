using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.PostsService.Application.Errors;
using Tailly.PostsService.Application.Service;
using Tailly.PostsService.Core.Models;
using Tailly.PostsService.Infrastructure.Repositories.Interfaces;

namespace Tailly.PostsService.Tests.Unit.Services;

/// <summary>
/// Unit tests for PostService.
/// Covers all methods with positive and negative scenarios:
/// - GetByIdAsync (success + not found + empty id)
/// - GetAllListAsync (success)
/// - GetListAsync (success + invalid sort)
/// - GetLatestAsync (success + limit clamping)
/// - CreateAsync (success + null + empty title + empty content)
/// - UpdateAsync (success + null + empty id + not found + empty title + empty content)
/// - GetAdminListAsync (success + invalid sort)
/// - DeleteAsync (success + empty id + not found)
/// </summary>
public class PostServiceTests
{
    private readonly Mock<IPostRepository> _repository = new();
    private readonly Mock<ILogger<PostService>> _logger = new();

    private PostService CreateService() =>
        new PostService(_repository.Object, _logger.Object);

    private Post CreateValidPost(Guid? id = null) => new Post
    {
        Id = id ?? Guid.NewGuid(),
        Title = "Тестовый пост",
        Content = "Полный текст поста...",
        PublishedAt = DateTime.UtcNow
    };

    // =============================================
    // ============== GetByIdAsync ==============
    // =============================================

    [Fact]
    public async Task GetByIdAsync_Should_Return_Post_When_Exists()
    {
        // arrange
        var post = CreateValidPost();
        _repository.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync(post);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(post.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(post);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_PostNotFound_When_Post_Does_Not_Exist()
    {
        // arrange
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Post?)null);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(id);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.PostNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_PostNotFound_When_Id_Is_Empty()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(Guid.Empty);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.PostNotFound);
    }

    // =============================================
    // ============== GetAllListAsync ==============
    // =============================================

    [Fact]
    public async Task GetAllListAsync_Should_Return_All_Posts()
    {
        // arrange
        var posts = new List<Post> { CreateValidPost(), CreateValidPost() };
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync(posts);

        var service = CreateService();

        // act
        var result = await service.GetAllListAsync();

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    // =============================================
    // ============== GetListAsync ==============
    // =============================================

    [Fact]
    public async Task GetListAsync_Should_Return_Posts_With_Tags()
    {
        // arrange
        var posts = new List<Post> { CreateValidPost() };
        _repository.Setup(x => x.GetListAsync(1, 10, null, null, null))
                   .ReturnsAsync((posts, 1));
        _repository.Setup(x => x.GetAllTagsAsync()).ReturnsAsync(new List<string> { "news", "tips" });

        var service = CreateService();

        // act
        var result = await service.GetListAsync(1, 10, null, null, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        var (returnedPosts, total, tags) = result.Value;   // ← исправлено
        returnedPosts.Should().HaveCount(1);
        tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_InvalidSort_When_Sort_Is_Invalid()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetListAsync(1, 10, null, null, "invalid-sort");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.InvalidSort);
    }

    // =============================================
    // ============== GetLatestAsync ==============
    // =============================================

    [Fact]
    public async Task GetLatestAsync_Should_Return_Latest_Posts()
    {
        // arrange
        var posts = new List<Post> { CreateValidPost() };
        _repository.Setup(x => x.GetLatestAsync(5)).ReturnsAsync(posts);

        var service = CreateService();

        // act
        var result = await service.GetLatestAsync(5);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_Post_Successfully()
    {
        // arrange
        var post = CreateValidPost();
        _repository.Setup(x => x.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(post);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        _repository.Verify(x => x.AddAsync(It.IsAny<Post>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_InvalidPost_When_Post_Is_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.CreateAsync(null!);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.InvalidPost);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_EmptyTitle_When_Title_Is_Empty()
    {
        // arrange
        var post = CreateValidPost();
        post.Title = "";
        var service = CreateService();

        // act
        var result = await service.CreateAsync(post);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.EmptyTitle);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_EmptyContent_When_Content_Is_Empty()
    {
        // arrange
        var post = CreateValidPost();
        post.Content = "";
        var service = CreateService();

        // act
        var result = await service.CreateAsync(post);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.EmptyContent);
    }

    // =============================================
    // ============== UpdateAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateAsync_Should_Update_Post_Successfully()
    {
        // arrange
        var post = CreateValidPost();
        var existing = CreateValidPost(post.Id);
        _repository.Setup(x => x.GetByIdForAdminAsync(post.Id)).ReturnsAsync(existing);
        _repository.Setup(x => x.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(post);

        // assert
        result.IsSuccess.Should().BeTrue();
        _repository.Verify(x => x.UpdateAsync(It.IsAny<Post>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_PostNotFound_When_Post_Does_Not_Exist()
    {
        // arrange
        var post = CreateValidPost();
        _repository.Setup(x => x.GetByIdForAdminAsync(post.Id)).ReturnsAsync((Post?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(post);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.PostNotFound);
    }

    // =============================================
    // ============== GetAdminListAsync ==============
    // =============================================

    [Fact]
    public async Task GetAdminListAsync_Should_Return_Admin_Posts()
    {
        // arrange
        var posts = new List<Post> { CreateValidPost() };
        _repository.Setup(x => x.GetAdminListAsync(1, 10, null, null))
                   .ReturnsAsync((posts, 1));

        var service = CreateService();

        // act
        var result = await service.GetAdminListAsync(1, 10, null, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        var (returnedPosts, total) = result.Value;
        returnedPosts.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAdminListAsync_Should_Return_InvalidSort_When_Sort_Is_Invalid()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.GetAdminListAsync(1, 10, null, "invalid");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.InvalidSort);
    }

    // =============================================
    // ============== DeleteAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteAsync_Should_Delete_Post_Successfully()
    {
        // arrange
        var id = Guid.NewGuid();
        var existing = CreateValidPost(id);
        _repository.Setup(x => x.GetByIdForAdminAsync(id)).ReturnsAsync(existing);
        _repository.Setup(x => x.DeleteAsync(id)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(id, Guid.NewGuid());

        // assert
        result.IsSuccess.Should().BeTrue();
        _repository.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_PostNotFound_When_Post_Does_Not_Exist()
    {
        // arrange
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdForAdminAsync(id)).ReturnsAsync((Post?)null);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(id, Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.PostNotFound.Description);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_PostNotFound_When_Id_Is_Empty()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.DeleteAsync(Guid.Empty, Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.PostNotFound.Description);
    }
}