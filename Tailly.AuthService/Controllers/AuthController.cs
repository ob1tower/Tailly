using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Tailly.AuthService.Dtos.Auth.Email;
using Tailly.AuthService.Dtos.Auth.Login;
using Tailly.AuthService.Dtos.Auth.Password;
using Tailly.AuthService.Dtos.Auth.Register;
using Tailly.AuthService.Dtos.Auth.Token;
using Tailly.AuthService.Errors;
using Tailly.AuthService.Service.Auth;

namespace Tailly.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ChangeEmailRequest> _changeEmailValidator;
    private readonly IValidator<ConfirmEmailChangeRequest> _confirmChangeEmailValidator;

    public AuthController(IAuthenticationService authService,
                          IValidator<RegisterRequest> registerValidator,
                          IValidator<LoginRequest> loginValidator,
                          IValidator<RefreshTokenRequest> refreshValidator,
                          IValidator<ChangePasswordRequest> changePasswordValidator,
                          IValidator<ResetPasswordRequest> resetPasswordValidator,
                          IValidator<ForgotPasswordRequest> forgotPasswordValidator,
                          IValidator<ChangeEmailRequest> changeEmailValidator,
                          IValidator<ConfirmEmailChangeRequest> confirmChangeEmailValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _changePasswordValidator = changePasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _changeEmailValidator = changeEmailValidator;
        _confirmChangeEmailValidator = confirmChangeEmailValidator;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// Sends a confirmation code to the provided email address.
    /// </summary>
    /// <param name="request">Registration data (email and password)</param>
    /// <returns>User ID of the newly created user</returns>
    [HttpPost("register")]
    [EnableRateLimiting("registration")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _authService.RegisterAsync(
            request.Email,
            request.Password);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { UserId = result.Value });
    }

    /// <summary>
    /// Confirms the user's email address using the verification code.
    /// </summary>
    /// <param name="request">Email and verification code</param>
    /// <returns>Success message if email was confirmed</returns>
    [HttpPost("confirm-email")]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var result = await _authService.ConfirmEmailAsync(
            request.Email,
            request.Code);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Authenticates a user and returns JWT access + refresh tokens.
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <returns>Access and refresh tokens with expiration dates</returns>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _authService.LoginAsync(
            request.Email,
            request.Password);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = new AuthResponse
        {
            AccessToken = result.Value.AccessToken,
            RefreshToken = result.Value.RefreshToken,
            AccessTokenExpires = result.Value.AccessTokenExpires,
            RefreshTokenExpires = result.Value.RefreshTokenExpires
        };

        return Ok(response);
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// Implements refresh token rotation for better security.
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New pair of access and refresh tokens</returns>
    [HttpPost("refresh")]
    [EnableRateLimiting("session")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = new AuthResponse
        {
            AccessToken = result.Value.AccessToken,
            RefreshToken = result.Value.RefreshToken,
            AccessTokenExpires = result.Value.AccessTokenExpires,
            RefreshTokenExpires = result.Value.RefreshTokenExpires
        };

        return Ok(response);
    }

    /// <summary>
    /// Logs out the user by invalidating the current refresh token.
    /// </summary>
    /// <param name="request">Refresh token to invalidate</param>
    [HttpPost("logout")]
    [EnableRateLimiting("session")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _authService.LogoutAsync(request.RefreshToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Initiates password reset process.
    /// Sends a reset code to the user's email (even if email doesn't exist - for security reasons).
    /// </summary>
    /// <param name="request">User email</param>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("registration")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var validation = await _forgotPasswordValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _authService.ForgotPasswordAsync(request.Email);

        return Ok();
    }

    /// <summary>
    /// Initiates password reset process.
    /// Sends a reset code to the user's email (even if email doesn't exist - for security reasons).
    /// </summary>
    /// <param name="request">User email</param>
    [HttpPost("reset-password")]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var validation = await _resetPasswordValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _authService.ResetPasswordAsync(
            request.Email,
            request.Code,
            request.NewPassword);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Changes password for currently authenticated user.
    /// Requires current password for verification.
    /// </summary>
    /// <param name="request">Current password and new password</param>
    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("session")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _authService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Requests to change the user's email address.
    /// Sends a confirmation code to the new email.
    /// </summary>
    /// <param name="request">New email address</param>
    [HttpPost("change-email/request")]
    [Authorize]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> RequestEmailChange([FromBody] ChangeEmailRequest request)
    {
        var validation = await _changeEmailValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _authService.RequestEmailChangeAsync(userId, request.NewEmail);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Confirms the email address change using the verification code.
    /// </summary>
    /// <param name="request">New email and confirmation code</param>
    [HttpPost("change-email/confirm")]
    [Authorize]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request)
    {
        var validation = await _confirmChangeEmailValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _authService.ConfirmEmailChangeAsync(
            userId,
            request.NewEmail,
            request.Code);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}