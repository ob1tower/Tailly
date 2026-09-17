using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.PostsService.Application.Dtos.Requests;
using Tailly.PostsService.Application.Errors;
using Tailly.PostsService.Application.Mappers;
using Tailly.PostsService.Application.Service.Interfaces;
using Tailly.PostsService.Application.Validators;
using Tailly.PostsService.Infrastructure.Configurations.Extensions;

namespace Tailly.PostsService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminPostController : ControllerBase
{
    private readonly IPostService _service;

    private readonly IValidator<CreatePostRequest> _createValidator;
    private readonly IValidator<UpdatePostRequest> _updateValidator;

    public AdminPostController(IPostService service,
                               IValidator<CreatePostRequest> createValidator,
                               IValidator<UpdatePostRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets posts.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10).</param>
    /// <param name="search">Search query for title.</param>
    /// <param name="sort">Sort: newest, oldest, title_asc, title_desc.</param>
    /// <returns>Paginated list of posts.</returns>
    [HttpGet("admin/content/posts")]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sort = null)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var pagination = new PaginationValidator(page, pageSize);

        var result = await _service.GetAdminListAsync(pagination.PageNumber, pagination.PageSize, search, sort);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (posts, total) = result.Value;

        return Ok(posts.Select(PostMapper.ToResponse));
    }

    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="request">The post creation data</param>
    /// <returns>The created post</returns>
    [HttpPost("admin/content/posts")]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var model = PostMapper.ToModel(request, userId.Value);

        var result = await _service.CreateAsync(model);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(PostMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Updates an existing post.
    /// </summary>
    /// <param name="id">The ID of the post to update</param>
    /// <param name="request">The updated post data</param>
    /// <returns>The updated post</returns>
    [HttpPut("admin/content/posts/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var model = PostMapper.ToModel(request, id, userId.Value);

        var result = await _service.UpdateAsync(model);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(PostMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Deletes a post by ID.
    /// </summary>
    /// <param name="id">The ID of the post to delete</param>
    /// <returns>Operation result</returns>
    [HttpDelete("admin/content/posts/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _service.DeleteAsync(id, userId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }
}