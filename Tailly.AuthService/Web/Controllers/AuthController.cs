using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests;
using Tailly.AuthService.Application.Dtos.Requests.EmailChange;
using Tailly.AuthService.Application.Dtos.Requests.Login;
using Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;
using Tailly.AuthService.Application.Dtos.Requests.Register;
using Tailly.AuthService.Application.Dtos.Requests.Token;
using Tailly.AuthService.Application.Dtos.Responses;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Auth.Interfaces;
using Tailly.AuthService.Infrastructure.Configurations.Extensions;

namespace Tailly.AuthService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    private readonly IValidator<RegisterStartRequest> _registerStartValidator;
    private readonly IValidator<RegisterVerifyRequest> _registerVerifyValidator;
    private readonly IValidator<RegisterCompleteRequest> _registerCompleteValidator;

    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;

    private readonly IValidator<StartPasswordRecoveryPayload> _startPasswordRecoveryValidator;
    private readonly IValidator<SendRecoveryCodePayload> _sendRecoveryCodeValidator;
    private readonly IValidator<VerifyRecoveryCodePayload> _verifyRecoveryCodeValidator;
    private readonly IValidator<ResetPasswordPayload> _resetPasswordValidator;

    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<RequestEmailChangeRequest> _requestEmailChangeValidator;
    private readonly IValidator<ConfirmEmailChangeRequest> _confirmEmailChangeValidator;

    public AuthController(IAuthenticationService authService,
                          IValidator<RegisterStartRequest> registerStartValidator,
                          IValidator<RegisterVerifyRequest> registerVerifyValidator,
                          IValidator<RegisterCompleteRequest> registerCompleteValidator,
                          IValidator<LoginRequest> loginValidator,
                          IValidator<RefreshTokenRequest> refreshValidator,
                          IValidator<StartPasswordRecoveryPayload> startPasswordRecoveryValidator,
                          IValidator<SendRecoveryCodePayload> sendRecoveryCodeValidator,
                          IValidator<VerifyRecoveryCodePayload> verifyRecoveryCodeValidator,
                          IValidator<ResetPasswordPayload> resetPasswordValidator,
                          IValidator<ChangePasswordRequest> changePasswordValidator,
                          IValidator<RequestEmailChangeRequest> requestEmailChangeValidator,
                          IValidator<ConfirmEmailChangeRequest> confirmEmailChangeValidator)
    {
        _authService = authService;

        _registerStartValidator = registerStartValidator;
        _registerVerifyValidator = registerVerifyValidator;
        _registerCompleteValidator = registerCompleteValidator;

        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;

        _startPasswordRecoveryValidator = startPasswordRecoveryValidator;
        _sendRecoveryCodeValidator = sendRecoveryCodeValidator;
        _verifyRecoveryCodeValidator = verifyRecoveryCodeValidator;
        _resetPasswordValidator = resetPasswordValidator;

        _changePasswordValidator = changePasswordValidator;
        _requestEmailChangeValidator = requestEmailChangeValidator;
        _confirmEmailChangeValidator = confirmEmailChangeValidator;
    }

    /// <summary>
    /// Starts the user registration process by sending a verification code to the email.
    /// </summary>
    /// <param name="request">Registration data containing email and password.</param>
    /// <returns>Registration session ID.</returns>
    [HttpPost("register/start")]
    [EnableRateLimiting("registration")]
    public async Task<IActionResult> StartRegister([FromBody] RegisterStartRequest request)
    {
        var validation = await _registerStartValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _authService.StartRegisterAsync(request.Email, request.Password);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { RegistrationId = result.Value });
    }

    /// <summary>
    /// Verifies the email using the code sent during registration.
    /// </summary>
    /// <param name="request">Contains registration ID and verification code.</param>
    /// <returns>Verification token required for completing registration.</returns>
    [HttpPost("register/verify")]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> VerifyRegister([FromBody] RegisterVerifyRequest request)
    {
        var validationResult = await _registerVerifyValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _authService.VerifyRegisterAsync(
            request.RegistrationId,
            request.Code);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { VerificationToken = result.Value });
    }

    /// <summary>
    /// Completes the registration process and issues access + refresh tokens.
    /// </summary>
    /// <param name="request">Contains registration ID and verification token.</param>
    /// <returns>Authentication result with tokens and user information.</returns>
    [HttpPost("register/complete")]
    [EnableRateLimiting("registration")]
    public async Task<IActionResult> CompleteRegister([FromBody] RegisterCompleteRequest request)
    {
        var validationResult = await _registerCompleteValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _authService.CompleteRegisterAsync(
            request.VerificationToken,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.CityName,
            request.CityId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new AuthResponse
        {
            AccessToken = result.Value.AccessToken,
            AccessTokenExpires = result.Value.AccessTokenExpires,
            RefreshToken = result.Value.RefreshToken,
            RefreshTokenExpires = result.Value.RefreshTokenExpires,
            User = result.Value.User
        });
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="request">Login credentials and requested role.</param>
    /// <returns>Authentication result containing access token and user data.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        if (!AuthMapper.TryParseRole(request.RequestedRole, out var role))
            return BadRequest(AuthErrors.InvalidRole);

        var result = await _authService.LoginAsync(request.Email, request.Password, role);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Auth.InvalidCredentials" => Unauthorized(result.Error),
                "Auth.AccountBlocked" => Unauthorized(result.Error),
                "Auth.EmailNotConfirmed" => Unauthorized(result.Error),
                "Auth.InvalidRole" => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="request">Refresh token.</param>
    /// <returns>New access and refresh tokens.</returns>
    [HttpPost("refresh")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var validationResult = await _refreshValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result.IsFailure)
            return Unauthorized(result.Error);

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
    /// Logs out the user by invalidating the refresh token.
    /// </summary>
    /// <param name="request">Refresh token to be invalidated.</param>
    [HttpPost("logout")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var validationResult = await _refreshValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _authService.LogoutAsync(request.RefreshToken);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        return Ok();
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

        var result = await _authService.StartPasswordRecoveryAsync(request.Email);

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

        var result = await _authService.SendRecoveryCodeAsync(request.Email);

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

        var result = await _authService.VerifyRecoveryCodeAsync(request.Email, request.Code);

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

        var result = await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="request">Current password and new password.</param>
    /// <returns>No content on success.</returns>
    [Authorize(Roles = "Client,Specialist")]
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

        var result = await _authService.ChangePasswordAsync(userId.Value, request.OldPassword, request.NewPassword);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Requests to change the user's email address.
    /// </summary>
    /// <param name="request">New email address.</param>
    /// <returns>Request ID and masked current email.</returns>
    [Authorize(Roles = "Client,Specialist")]
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

        var result = await _authService.RequestEmailChangeAsync(userId.Value, request.NewEmail);

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
    [Authorize(Roles = "Client,Specialist")]
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

        var result = await _authService.ConfirmEmailChangeAsync(userId.Value, request.RequestId,
                                                                request.NewEmail, request.Code);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}