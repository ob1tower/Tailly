using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests.SuperAdmin;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.SuperAdmin;
using Tailly.AuthService.Application.Validators;
using Tailly.AuthService.Application.Validators.SuperAdmin;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Configurations.Extensions;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminController : ControllerBase
{
    private readonly CreateAdminRequestValidator _createValidator;
    private readonly UpdateAdminRequestValidator _updateValidator;
    private readonly UpdateAdminBlockStatusValidator _blockValidator;
    private readonly ISuperAdminService _service;

    public SuperAdminController(ISuperAdminService service,
                                CreateAdminRequestValidator createValidator,
                                UpdateAdminRequestValidator updateValidator,
                                UpdateAdminBlockStatusValidator blockValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _blockValidator = blockValidator;
    }

    /// <summary>
    /// Get a list of administrators.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>List of administrators.</returns>
    [HttpGet("super-admin/admins")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var pagination = new PaginationValidator(page, pageSize);

        var result = await _service.GetAllAsync(pagination.PageNumber, pagination.PageSize);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Items);
    }

    /// <summary>
    /// Create a new administrator.
    /// </summary>
    /// <param name="request">Administrator creation data.</param>
    /// <returns>Created administrator and temporary password.</returns>
    [HttpPost("super-admin/admins")]
    public async Task<IActionResult> Create([FromBody] CreateAdminRequest request)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _service.CreateAsync(
            request.Email,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.BirthDate,
            request.Phone,
            AuthMapper.ParseDepartment(request.Department));

        if (result.IsFailure)
            return BadRequest(result.Error);

        (ManagedAdmin admin, string tempPassword) = result.Value;

        return Ok(new
        {
            admin,
            temporaryPassword = tempPassword
        });
    }

    /// <summary>
    /// Delete an administrator.
    /// </summary>
    /// <param name="adminId">Administrator ID.</param>
    /// <returns>The result of the operation.</returns>
    [HttpDelete("super-admin/admins/{adminId:guid}")]
    public async Task<IActionResult> Delete(Guid adminId)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _service.DeleteAsync(adminId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Update administrator data.
    /// </summary>
    /// <param name="adminId">Administrator ID.</param>
    /// <param name="request">Updated administrator data.</param>
    /// <returns>Updated administrator.</returns>
    [HttpPatch("super-admin/admins/{adminId:guid}")]
    public async Task<IActionResult> Update(Guid adminId, [FromBody] UpdateAdminRequest request)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _service.UpdateAsync(
            adminId,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.BirthDate,
            request.Phone,
            AuthMapper.ParseDepartment(request.Department));

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Update administrator block status.
    /// </summary>
    /// <param name="adminId">Administrator ID.</param>
    /// <param name="request">Block status data.</param>
    /// <returns>The result of the operation.</returns>
    [EnableRateLimiting("admin-actions")]
    [HttpPatch("super-admin/admins/{adminId:guid}/block")]
    public async Task<IActionResult> UpdateBlockStatus(Guid adminId, [FromBody] UpdateAdminBlockStatusPayload request)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var validationResult = await _blockValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _service.UpdateBlockStatusAsync(
            adminId,
            request.IsBlocked,
            request.BlockReason,
            request.BlockedUntil,
            request.IsPermanentBlock);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Clear temporary password attempts lock for administrator.
    /// </summary>
    /// <param name="adminId">Administrator ID.</param>
    /// <returns>The result of the operation.</returns>
    [EnableRateLimiting("admin-actions")]
    [HttpDelete("super-admin/admins/{adminId:guid}/password-attempts-lock")]
    public async Task<IActionResult> ClearPasswordLock(Guid adminId)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _service.ClearPasswordAttemptsLockAsync(adminId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Get list of admin password recovery requests.
    /// </summary>
    /// <returns>List of recovery requests.</returns>
    [HttpGet("super-admin/password-recovery-requests")]
    public async Task<IActionResult> GetPasswordRecoveryRequests()
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _service.GetPasswordRecoveryAsync();

        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = result.Value.Select(r => new
        {
            r.Id,
            r.Email,
            r.RequestedAt,
            Status = r.Status == AdminPasswordRecoveryStatus.Processed ? "processed" : "pending",
            r.ProcessedAt,
            r.TemporaryPassword
        });

        return Ok(response);
    }

    /// <summary>
    /// Process password recovery request and send new temporary password to admin.
    /// </summary>
    /// <param name="requestId">Recovery request ID.</param>
    /// <returns>Temporary password and email.</returns>
    [HttpPost("super-admin/password-recovery-requests/{requestId:guid}/process")]
    public async Task<IActionResult> ProcessPasswordRecoveryRequest(Guid requestId)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _service.ProcessPasswordRecoveryAsync(requestId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}