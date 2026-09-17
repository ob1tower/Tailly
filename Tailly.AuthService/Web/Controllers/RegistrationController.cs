using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests.Register;
using Tailly.AuthService.Application.Dtos.Responses;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Registration;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly IValidator<RegisterStartRequest> _registerStartValidator;
    private readonly IValidator<RegisterVerifyRequest> _registerVerifyValidator;
    private readonly IValidator<RegisterCompleteRequest> _registerCompleteValidator;

    public RegistrationController(IRegistrationService registrationService,
                                  IValidator<RegisterStartRequest> registerStartValidator,
                                  IValidator<RegisterVerifyRequest> registerVerifyValidator,
                                  IValidator<RegisterCompleteRequest> registerCompleteValidator)
    {
        _registrationService = registrationService;
        _registerStartValidator = registerStartValidator;
        _registerVerifyValidator = registerVerifyValidator;
        _registerCompleteValidator = registerCompleteValidator;
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

        var result = await _registrationService.StartRegisterAsync(request.Email, request.Password);

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

        var result = await _registrationService.VerifyRegisterAsync(
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

        var result = await _registrationService.CompleteRegisterAsync(
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
}