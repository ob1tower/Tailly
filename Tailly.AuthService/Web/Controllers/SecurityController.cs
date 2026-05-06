using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests;
using Tailly.AuthService.Application.Dtos.Requests.Admin;
using Tailly.AuthService.Application.Dtos.Requests.EmailChange;
using Tailly.AuthService.Application.Dtos.Responses;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Security;
using Tailly.AuthService.Infrastructure.Configurations.Extensions;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SecurityController : ControllerBase
{
    private readonly IUserSecurityService _userSecurityService;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<EmailChangeRequest> _requestEmailChangeValidator;
    private readonly IValidator<ConfirmEmailChangeRequest> _confirmEmailChangeValidator;

    public SecurityController(IUserSecurityService userSecurityService,
                              IValidator<ChangePasswordRequest> changePasswordValidator,
                              IValidator<EmailChangeRequest> requestEmailChangeValidator,
                              IValidator<ConfirmEmailChangeRequest> confirmEmailChangeValidator)
    {
        _userSecurityService = userSecurityService;
        _changePasswordValidator = changePasswordValidator;
        _requestEmailChangeValidator = requestEmailChangeValidator;
        _confirmEmailChangeValidator = confirmEmailChangeValidator;
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="request">Current password and new password.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("me/security/password/change")]
    [EnableRateLimiting("token")]
    [Authorize(Roles = "Client,Specialist")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _userSecurityService.ChangePasswordAsync(userId.Value, request.OldPassword, request.NewPassword);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Requests to change the user's email address.
    /// </summary>
    /// <param name="request">New email address.</param>
    /// <returns>Request ID and masked current email.</returns>
    [HttpPost("me/security/email/change/request")]
    [EnableRateLimiting("token")]
    [Authorize(Roles = "Client,Specialist")]
    public async Task<IActionResult> RequestEmailChange([FromBody] EmailChangeRequest request)
    {
        var validationResult = await _requestEmailChangeValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _userSecurityService.RequestEmailChangeAsync(userId.Value, request.NewEmail);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = new EmailChangeResponse
        {
            RequestId = result.Value.RequestId,
            MaskedOldEmail = result.Value.MaskedOldEmail
        };

        return Ok(response);
    }

    /// <summary>
    /// Confirms the email change using the verification code.
    /// </summary>
    /// <param name="request">Request ID, new email and verification code.</param>
    [HttpPost("me/security/email/change/confirm")]
    [EnableRateLimiting("verification")]
    [Authorize(Roles = "Client,Specialist")]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request)
    {
        var validationResult = await _confirmEmailChangeValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _userSecurityService.ConfirmEmailChangeAsync(userId.Value, request.RequestId,
                                                                request.NewEmail, request.Code);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Request email change for super admin (requires current password).
    /// </summary>
    [HttpPost("admin/profile/email-change/request")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RequestEmailChange([FromBody] EmailChangePayloadRequest payload)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _userSecurityService.EmailChangeAsync(
            userId.Value,
            payload.NewEmail,
            payload.Password);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Confirm email change with verification code (SuperAdmin).
    /// </summary>
    [HttpPost("admin/profile/email-change/confirm")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangePayloadRequest payload)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _userSecurityService.ConfirmEmailChangeAsync(
            userId.Value,
            payload.Code);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Cancel pending email change request (SuperAdmin).
    /// </summary>
    [HttpDelete("admin/profile/email-change")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CancelEmailChange()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _userSecurityService.CancelEmailChangeAsync(userId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Change password for admin / super admin.
    /// </summary>
    /// <param name="request">Current password and new password.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("admin/security/password/change")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ChangeAdminPassword([FromBody] ChangePasswordRequest request)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _userSecurityService.ChangePasswordAsync(
            userId.Value,
            request.OldPassword,
            request.NewPassword);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}