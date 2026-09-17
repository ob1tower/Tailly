using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests.Admin;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Admin;
using Tailly.AuthService.Application.Validators;
using Tailly.AuthService.Application.Validators.Admin;
using Tailly.AuthService.Infrastructure.Configurations.Extensions;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly UpdateUserBlockStatusValidator _blockValidator;
    private readonly UpdateUserProfileValidator _profileValidator;

    public AdminUsersController(IAdminUserService adminUserService,
                                UpdateUserBlockStatusValidator blockValidator,
                                UpdateUserProfileValidator profileValidator)
    {
        _adminUserService = adminUserService;
        _blockValidator = blockValidator;
        _profileValidator = profileValidator;
    }

    /// <summary>
    /// Get a list of users.
    /// </summary>
    /// <param name="search">The search bar.</param>
    /// <param name="role">Filter by role (client / specialist).</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>List of users.</returns>
    [HttpGet("admin/users")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? role, [FromQuery] int page = 1, [FromQuery]  int pageSize = 20)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

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

    /// <summary>
    /// Get a user by ID and role.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="role">Role.</param>
    /// <returns>User data.</returns>
    [HttpGet("admin/users/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string role)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _adminUserService.GetByIdAsync(id, role);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Update the user's blocking status.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="role">Role.</param>
    /// <param name="request">Blocking parameters.</param>
    /// <returns>The result of the operation.</returns>
    [EnableRateLimiting("admin-actions")]
    [HttpPatch("admin/users/{id:guid}/roles/{role}/block-status")]
    public async Task<IActionResult> UpdateBlockStatus(Guid id, string role, [FromBody] UpdateUserBlockStatusRequest request)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        if (!AuthMapper.TryParseRole(role, out var parsedRole))
            return BadRequest(AuthErrors.InvalidRole);

        var validationResult = await _blockValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _adminUserService.UpdateBlockStatusAsync(
            id,
            parsedRole,
            request.IsBlocked,
            request.IsPermanentBlock,
            request.BlockedUntil,
            request.BlockReason);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Restore the user's role from the deletion state.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="role">Role.</param>
    /// <returns>The result of the operation.</returns>
    [EnableRateLimiting("admin-actions")]
    [HttpPost("admin/users/{id:guid}/roles/{role}/restore-from-deletion")]
    public async Task<IActionResult> Restore(Guid id, string role)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _adminUserService.RestoreFromDeletionAsync(id, role);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Update user profile.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="request">Profile data.</param>
    /// <returns>The result of the operation.</returns>
    [EnableRateLimiting("admin-actions")]
    [HttpPatch("admin/users/{id:guid}/profile")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateUserProfileRequest request)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var validationResult = await _profileValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _adminUserService.UpdateProfileAsync(id, request.FirstName, request.LastName, request.MiddleName, request.SpecialistSlug);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}