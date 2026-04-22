using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.AuthService.Application.Dtos.Requests.Admin;
using Tailly.AuthService.Application.Service.Admin;
using Tailly.AuthService.Application.Validators;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("admin/users")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? role, [FromQuery] int page = 1, [FromQuery]  int pageSize = 20)
    {
        var pagination = new PaginationValidator(page, pageSize);

        var result = await _adminUserService.GetAllAsync(
            search,
            role,
            pagination.PageNumber,
            pagination.PageSize);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("admin/users/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _adminUserService.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPatch("admin/users/{id:guid}/block-status")]
    public async Task<IActionResult> UpdateBlockStatus(Guid id, [FromBody] UpdateUserBlockStatusRequest request)
    {
        var result = await _adminUserService.UpdateBlockStatusAsync(
            id,
            request.IsBlocked,
            request.IsPermanentBlock,
            request.BlockedUntil,
            request.BlockReason);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    [HttpPost("admin/users/{id:guid}/restore-from-deletion")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _adminUserService.RestoreFromDeletionAsync(id);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}