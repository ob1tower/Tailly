using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.PostsService.Application.Dtos.Requests;
using Tailly.PostsService.Application.Errors;
using Tailly.PostsService.Application.Mappers;
using Tailly.PostsService.Application.Service.Interfaces;
using Tailly.PostsService.Application.Validators;

namespace Tailly.PostsService.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminBannerController : ControllerBase
{
    private readonly IBannerService _service;
    private readonly IValidator<SaveBannerRequest> _validator;

    public AdminBannerController(IBannerService service, IValidator<SaveBannerRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>
    /// Gets list of banners.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10).</param>
    /// <param name="search">Search query for title.</param>
    /// <param name="sort">Sort: newest, oldest, title_asc, title_desc.</param>
    /// <returns>Paginated list of banners.</returns>
    [HttpGet("admin/content/banners")]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sort = null)
    {
        var pagination = new PaginationValidator(page, pageSize);

        var result = await _service.GetListAsync(pagination.PageNumber, pagination.PageSize, search, sort);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (banners, total) = result.Value;

        return Ok(new
        {
            items = banners.Select(BannerMapper.ToResponse),
            total,
            page = pagination.PageNumber,
            pageSize = pagination.PageSize
        });
    }

    /// <summary>
    /// Creates a new banner.
    /// </summary>
    /// <param name="request">Banner creation data.</param>
    /// <returns>Created banner.</returns>
    [HttpPost("admin/content/banners")]
    public async Task<IActionResult> Create([FromBody] SaveBannerRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var banner = BannerMapper.ToModel(request);

        var result = await _service.CreateAsync(banner);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(BannerMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Updates an existing banner.
    /// </summary>
    /// <param name="id">Banner ID.</param>
    /// <param name="request">Updated banner data.</param>
    /// <returns>Updated banner.</returns>
    [HttpPut("admin/content/banners/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveBannerRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var banner = BannerMapper.ToModel(request, id);

        var result = await _service.UpdateAsync(banner);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(BannerMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Deletes a banner.
    /// </summary>
    /// <param name="id">Banner ID.</param>
    /// <returns>Operation result.</returns>
    [HttpDelete("admin/content/banners/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }
}
