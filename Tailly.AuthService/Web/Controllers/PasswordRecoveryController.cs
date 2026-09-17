using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.PasswordRecovery;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PasswordRecoveryController : ControllerBase
{
    private readonly IPasswordRecoveryService _passwordRecoveryService;
    private readonly IValidator<StartPasswordRecoveryPayload> _startPasswordRecoveryValidator;
    private readonly IValidator<SendRecoveryCodePayload> _sendRecoveryCodeValidator;
    private readonly IValidator<VerifyRecoveryCodePayload> _verifyRecoveryCodeValidator;
    private readonly IValidator<ResetPasswordPayload> _resetPasswordValidator;

    public PasswordRecoveryController(IPasswordRecoveryService passwordRecoveryService,
                                      IValidator<StartPasswordRecoveryPayload> startPasswordRecoveryValidator,
                                      IValidator<SendRecoveryCodePayload> sendRecoveryCodeValidator,
                                      IValidator<VerifyRecoveryCodePayload> verifyRecoveryCodeValidator,
                                      IValidator<ResetPasswordPayload> resetPasswordValidator)
    {
        _passwordRecoveryService = passwordRecoveryService;
        _startPasswordRecoveryValidator = startPasswordRecoveryValidator;
        _sendRecoveryCodeValidator = sendRecoveryCodeValidator;
        _verifyRecoveryCodeValidator = verifyRecoveryCodeValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    /// <summary>
    /// Starts the password recovery process.
    /// </summary>
    /// <param name="request">Email of the user who wants to recover password.</param>
    /// <returns>Information about the recovery flow.</returns>
    [HttpPost("password-recovery/start")]
    [EnableRateLimiting("password-recovery")]
    public async Task<IActionResult> StartPasswordRecovery([FromBody] StartPasswordRecoveryPayload request)
    {
        var validationResult = await _startPasswordRecoveryValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _passwordRecoveryService.StartPasswordRecoveryAsync(request.Email);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { flow = result.Value.Flow });
    }

    /// <summary>
    /// Sends a verification code to the email for password recovery.
    /// </summary>
    /// <param name="request">Email to send the recovery code to.</param>
    [HttpPost("password-recovery/send-code")]
    [EnableRateLimiting("password-recovery")]
    public async Task<IActionResult> SendRecoveryCode([FromBody] SendRecoveryCodePayload request)
    {
        var validationResult = await _sendRecoveryCodeValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _passwordRecoveryService.SendRecoveryCodeAsync(request.Email);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Verifies the recovery code sent to the email.
    /// </summary>
    /// <param name="request">Email and verification code.</param>
    [HttpPost("password-recovery/verify-code")]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> VerifyRecoveryCode([FromBody] VerifyRecoveryCodePayload request)
    {
        var validationResult = await _verifyRecoveryCodeValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _passwordRecoveryService.VerifyRecoveryCodeAsync(request.Email, request.Code);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Resets the user's password using a valid recovery code.
    /// </summary>
    /// <param name="request">Email, recovery code and new password.</param>
    [HttpPost("password-recovery/reset")]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordPayload request)
    {
        var validationResult = await _resetPasswordValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _passwordRecoveryService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}