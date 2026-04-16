using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.PostsService.Application.Mappers;
using Tailly.PostsService.Application.Service.Interfaces;
using Tailly.PostsService.Application.Validators;

namespace Tailly.PostsService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _service;

    public PostController(IPostService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets paginated list of posts.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10).</param>
    /// <param name="search">Search query for title/content.</param>
    /// <param name="tag">Filter posts by tag.</param>
    /// <param name="sort">Sorting mode: newest, oldest, title_asc, title_desc.</param>
    /// <returns>Paginated list of posts.</returns>
    [HttpGet("posts")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? tag = null,
    [FromQuery] string? sort = null)
    {

        var pagination = new PaginationValidator(page, pageSize);

        var result = await _service.GetListAsync(
            pagination.PageNumber,
            pagination.PageSize,
            search,
            tag,
            sort);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (posts, total, availableTags) = result.Value;

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pagination.PageSize);

        return Ok(new
        {
            items = posts.Select(PostMapper.ToResponse),
            total,
            page = pagination.PageNumber,
            pageSize = pagination.PageSize,
            totalPages,
            availableTags
        });
    }

    /// <summary>
    /// Gets a single post by ID.
    /// </summary>
    /// <param name="id">Post ID.</param>
    /// <returns>Post data.</returns>
    [HttpGet("posts/{id:guid}")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(PostMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Gets latest published posts.
    /// </summary>
    /// <param name="limit">Maximum number of posts (default: 5).</param>
    /// <returns>List of latest posts.</returns>
    [HttpGet("posts/latest")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetLatest([FromQuery] int limit = 5)
    {
        var result = await _service.GetLatestAsync(limit);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Select(PostMapper.ToResponse));
    }
}