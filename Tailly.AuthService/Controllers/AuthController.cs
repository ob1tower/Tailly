using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Dtos.Auth;
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

    public AuthController(IAuthenticationService authService,
                          IValidator<RegisterRequest> registerValidator,
                          IValidator<LoginRequest> loginValidator,
                          IValidator<RefreshTokenRequest> refreshValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var result = await _authService.RegisterAsync(
            request.Email,
            request.Password);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { UserId = result.Value });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(validation.Errors);

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

    [HttpPost("refresh")]
    [EnableRateLimiting("session")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(validation.Errors);

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

    [HttpPost("logout")]
    [EnableRateLimiting("session")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var result = await _authService.LogoutAsync(request.RefreshToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}