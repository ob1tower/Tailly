using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Mappers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Application.Validators;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminSpecialistApplicationController : ControllerBase
{
    private readonly ISpecialistApplicationService _service;

    private readonly IValidator<AssignInterviewRequest> _assignValidator;
    private readonly IValidator<RejectApplicationRequest> _rejectValidator;
    private readonly IValidator<AttachSpecialistAccountRequest> _attachValidator;
    private readonly IValidator<ApproveApplicationRequest> _approveValidator;

    public AdminSpecialistApplicationController(ISpecialistApplicationService service,
                                                IValidator<AssignInterviewRequest> assignValidator,
                                                IValidator<RejectApplicationRequest> rejectValidator,
                                                IValidator<AttachSpecialistAccountRequest> attachValidator,
                                                IValidator<ApproveApplicationRequest> approveValidator)
    {
        _service = service;
        _assignValidator = assignValidator;
        _rejectValidator = rejectValidator;
        _attachValidator = attachValidator;
        _approveValidator = approveValidator;
    }

    /// <summary>
    /// Gets specialist applications.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Items per page (default: 20).</param>
    /// <param name="status">Status: pending_review, interview_assigned, approved, rejected.</param>
    [HttpGet("admin/specialist-applications")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] string? status = null)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var pagination = new PaginationValidator(page, limit);

        var statusEnum = string.IsNullOrEmpty(status)
        ? (SpecialistApplicationStatus?)null
        : SpecialistEnumMapper.ParseApplicationStatus(status);

        var result = await _service.GetAllAsync(
            pagination.PageNumber,
            pagination.PageSize,
            statusEnum);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (applications, total) = result.Value;

        var items = applications.Select(SpecialistApplicationMapper.ToListItem);

        return Ok(new
        {
            items,
            total,
            page = pagination.PageNumber,
            limit = pagination.PageSize
        });
    }

    /// <summary>
    /// Assigns an interview to a specialist application.
    /// </summary>
    /// <param name="id">Application ID.</param>
    /// <param name="request">Interview details (note + date).</param>
    [HttpPost("admin/specialist-applications/{id:guid}/assign-interview")]
    public async Task<IActionResult> AssignInterview(Guid id, [FromBody] AssignInterviewRequest request)
    {
        var validation = await _assignValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var reviewedBy = request.ReviewedBy
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.Identity?.Name
            ?? userId.ToString()!;

        var result = await _service.AssignInterviewAsync(id, request.Note, request.InterviewDate, reviewedBy);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Approves a specialist application.
    /// Optionally adds a review comment from the admin.
    /// </summary>
    /// <param name="id">Application ID.</param>
    /// <param name="request">Review comment (optional) and reviewed by admin (required).</param>
    [HttpPost("admin/specialist-applications/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveApplicationRequest request)
    {
        var validation = await _approveValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var reviewedBy = request.ReviewedBy
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.Identity?.Name
            ?? userId.ToString()!;

        var result = await _service.ApproveAsync(id, reviewedBy, request.ReviewComment);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Rejects a specialist application.
    /// </summary>
    /// <param name="id">Application ID.</param>
    /// <param name="request">Rejection reason.</param>
    [HttpPost("admin/specialist-applications/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectApplicationRequest request)
    {
        var validation = await _rejectValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var reviewedBy = request.ReviewedBy
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.Identity?.Name
            ?? userId.ToString()!;

        var result = await _service.RejectAsync(id, request.Reason, reviewedBy);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Creates a specialist account from an approved application.
    /// </summary>
    /// <param name="id">Application ID.</param>
    /// <param name="request">Reviewed by admin info.</param>
    [HttpPost("admin/specialist-applications/{id:guid}/create-specialist-account")]
    public async Task<IActionResult> AttachSpecialistAccount(Guid id, [FromBody] AttachSpecialistAccountRequest request)
    {
        var validation = await _attachValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _service.AttachSpecialistAccountAsync(id, request.ReviewedBy);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var specialist = result.Value.Specialist;

        return Ok(new
        {
            success = true,
            specialistId = specialist.Id.ToString(),
            temporaryPassword = result.Value.TemporaryPassword
        });
    }
}