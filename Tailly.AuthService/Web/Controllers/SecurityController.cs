using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests;
using Tailly.AuthService.Application.Dtos.Requests.EmailChange;
using Tailly.AuthService.Application.Dtos.Responses;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Security;
using Tailly.AuthService.Infrastructure.Configurations.Extensions;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client, Specialist")]
public class SecurityController : ControllerBase
{
    private readonly IUserSecurityService _userSecurityService;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<RequestEmailChangeRequest> _requestEmailChangeValidator;
    private readonly IValidator<ConfirmEmailChangeRequest> _confirmEmailChangeValidator;

    public SecurityController(IUserSecurityService userSecurityService,
                              IValidator<ChangePasswordRequest> changePasswordValidator,
                              IValidator<RequestEmailChangeRequest> requestEmailChangeValidator,
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
    public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeRequest request)
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
}